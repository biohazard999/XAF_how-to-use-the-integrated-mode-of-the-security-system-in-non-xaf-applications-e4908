﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreMvcApplication {
	public class Startup {
		private string loginPath = "/Authentication";
		public Startup(IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services) {
			services.AddMvc(options => {
				options.EnableEndpointRouting = false;
			})
				.SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
				.AddDxSampleModelJsonOptions();
			services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				 .AddCookie(options => {
					 options.LoginPath = loginPath;
				 });
			services.AddSingleton<XpoDataStoreProviderService>();
			services.AddSingleton(Configuration);
			services.AddHttpContextAccessor();
			services.AddScoped<SecurityProvider>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
			if(env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}
			else {
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			app.UseAuthentication();
			app.UseDefaultFiles();
			app.UseHttpsRedirection();
			app.UseStaticFiles(new StaticFileOptions() { 
				OnPrepareResponse = context => {
					if(context.Context.User.Identity.IsAuthenticated) {
						return;
					}
					else {
						string referer = context.Context.Request.Headers["Referer"].ToString();
						string authenticationPagePath = loginPath;
						string vendorString = "vendor.css";
						if(context.Context.Request.Path.HasValue && context.Context.Request.Path.StartsWithSegments(authenticationPagePath)
							|| referer != null && (referer.Contains(authenticationPagePath) || referer.Contains(vendorString))) {
							return;
						}
						context.Context.Response.Redirect(loginPath);
					}
				}
			});
			app.UseCookiePolicy();
			app.UseMvc(routes => {
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
