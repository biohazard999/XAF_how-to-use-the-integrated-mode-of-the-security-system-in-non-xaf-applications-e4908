﻿Imports DevExpress.ExpressApp
Imports DevExpress.ExpressApp.Security
Imports DevExpress.ExpressApp.Security.ClientServer
Imports DevExpress.ExpressApp.Xpo
Imports DevExpress.Persistent.BaseImpl
Imports DevExpress.Persistent.BaseImpl.PermissionPolicy
Imports System
Imports System.Configuration
Imports System.IO
Imports System.Linq
Imports System.Text
Imports XafSolution.Module.BusinessObjects

Namespace ConsoleApplication
    Friend Class Program
        Shared Sub Main()
            RegisterEntities()
            Dim authentication As New AuthenticationStandard()
            Dim security As New SecurityStrategyComplex(GetType(PermissionPolicyUser), GetType(PermissionPolicyRole), authentication)
            security.RegisterXPOAdapterProviders()

            Dim connectionString As String = ConfigurationManager.ConnectionStrings("ConnectionString").ConnectionString
            Dim objectSpacesProvider As New SecuredObjectSpaceProvider(security, connectionString, Nothing)

            DevExpress.Persistent.Base.PasswordCryptographer.EnableRfc2898 = True
            DevExpress.Persistent.Base.PasswordCryptographer.SupportLegacySha512 = False

            Dim userName As String = "User"
            Dim password As String = String.Empty
            authentication.SetLogonParameters(New AuthenticationStandardLogonParameters(userName, password))
            Dim loginObjectSpace As IObjectSpace = objectSpacesProvider.CreateObjectSpace()
            security.Logon(loginObjectSpace)

			Using file As New StreamWriter("result.txt", False)
				Dim stringBuilder As New StringBuilder()
				stringBuilder.Append(String.Format("{0} is logged on." & vbLf, userName))
				stringBuilder.Append("List of the 'Employee' objects:" & vbLf)
				Using securedObjectSpace As IObjectSpace = objectSpacesProvider.CreateObjectSpace()
					For Each employee As Employee In securedObjectSpace.GetObjects(Of Employee)()
						stringBuilder.Append("=========================================" & vbLf)
						stringBuilder.Append(String.Format("Full name: {0}" & vbLf, employee.FullName))
						If security.CanRead(Of Employee)(employee.Oid, securedObjectSpace, NameOf(Department)) Then
							stringBuilder.Append(String.Format("Department: {0}" & vbLf, employee.Department.Title))
						Else
							stringBuilder.Append("Department: [Protected content]" & vbLf)
						End If
					Next employee
				End Using
				file.Write(stringBuilder)
			End Using
			Console.WriteLine(String.Format("The result.txt file has been created in the {0} directory.", Environment.CurrentDirectory))
			Console.WriteLine("Press any key to close a the console...")
			Console.ReadLine()
        End Sub
        Private Shared Sub RegisterEntities()
            XpoTypesInfoHelper.GetXpoTypeInfoSource()
            XafTypesInfo.Instance.RegisterEntity(GetType(Employee))
            XafTypesInfo.Instance.RegisterEntity(GetType(PermissionPolicyUser))
            XafTypesInfo.Instance.RegisterEntity(GetType(PermissionPolicyRole))
        End Sub
    End Class
End Namespace