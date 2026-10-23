# Additional 1.5 integration tests

Compile each *IntegrationCheck.cs as an x64 .NET Framework console executable referencing HDLSS5-test.exe plus WPF assemblies. Invoke it with a fresh writable fixture directory containing HDLSS5-test.exe.

- ASI: asi.zip = official v10.0.0 NoPDB x64 archive; old/dinput8.dll = official v9.7.4 x64 DLL.
- F5: f5.zip = official v0.1.33 full archive; release.json = matching GitHub release metadata object (includes SHA256).
- Progress: no additional inputs.
- Visibility: source/Main.xaml = candidate XAML.

Only fixture game folders are changed. Do not point these tests at an actual game installation.
