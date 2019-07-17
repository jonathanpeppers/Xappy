param (
    [string] $msbuild = 'msbuild'
)

& git clean -dxf
& adb uninstall com.microsoft.Xappy
& adb uninstall Mono.Android.DebugRuntime
& adb uninstall Mono.Android.Platform.ApiLevel_28
& adb logcat -c

& $msbuild .\Xappy\Xappy.Android\Xappy.Android.csproj /t:Install,_Run /r

& adb logcat | Select-String Displayed

# Jon Peppers initial results, seems fine?

# 16.1
# 07-17 18:51:34.044  1196  1209 I ActivityManager: Displayed com.microsoft.Xappy/md5dc8e1b02975c3158365aa81cf255d5af.MainActivity: +2s352ms
# 16.2
# 07-17 18:52:50.866  1196  1209 I ActivityManager: Displayed com.microsoft.Xappy/md5dc8e1b02975c3158365aa81cf255d5af.MainActivity: +2s327ms
# 16.3
# 07-17 18:53:50.858  1196  1209 I ActivityManager: Displayed com.microsoft.Xappy/md5dc8e1b02975c3158365aa81cf255d5af.MainActivity: +2s327ms