# Protocol Interceptor - 协议拦截器

拦截 HTTP(S) 协议的超链接，在 Windows 操作系统中触发的打开默认浏览器的行为，
并利用 Windows 10.0.17763.0 的通知功能，弹出原生通知框，让用户决定是否跳转。

## 使用

1. 正常安装
2. 运行 `ms-settings:defaultapps?registeredAppUser=Protocol+Interceptor` 设置默认应用为 `协议拦截器`
3. 开始使用

## 参考文档
- 应用通知 - [App notifications overview - Windows apps | Microsoft Learn](https://learn.microsoft.com/en-us/windows/apps/develop/notifications/app-notifications/)
- 设置默认应用 - [Launch the Default Apps settings page - Windows apps | Microsoft Learn](https://learn.microsoft.com/en-us/windows/apps/develop/launch/launch-default-apps-settings)
- 文件打开方式弹窗的参数 - [OPENASINFO (shlobj_core.h) - Win32 apps | Microsoft Learn](https://learn.microsoft.com/en-us/windows/win32/api/shlobj_core/ns-shlobj_core-openasinfo)
- 文件打开方式弹窗 - [SHOpenWithDialog function (shlobj_core.h) - Win32 apps | Microsoft Learn](https://learn.microsoft.com/en-us/windows/win32/api/shlobj_core/nf-shlobj_core-shopenwithdialog)
