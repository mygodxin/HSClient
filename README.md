# HSClient

简易Unity框架，方便开发。由HybridCLR+YooAsset+Luban组成
主要功能包括：
 1. 代码和资源热更新
 2. 新手引导组件，支持圆形以及矩形反向遮罩
 3. 界面组件自动绑定导出对应代码，以及配套的UI加载、打开关闭、层级处理、缓存，事件处理等
 4. 音频管理器，播放音乐以及音效
 5. 事件管理器，支持全局事件，单个事件
 6. 配置管理器，包括配置文件加载，配置文件缓存
 7. 计时器组件，实现了类似js中SetInterval和SetTimeout
 8. UI序列帧组件，支持以文件夹为单位快速创建序列帧，以及编辑器中预览对应效果
 9. 常用工具，状态机，四叉树检测，A星寻路
一、游戏热更流程
1. 参考HybridCLR教程，安装以及配置
2. 运行HybridCLR，将生成的DLL拷贝至Assets/HotfixPackages/HotfixDll目录下
3. 参考YooAsset教程，安装以及配置
4. 运行YooAsset中的构建
5. 将构建出来的包放入CDN中，并配置CDN地址至ResUpdate.cs文件里GetHostServerURL方法中的hostServerIP
6. 构建出包
7. 修改资源或者脚本后，运行YooAsset中的构建打出新包放到CDN即可验证热更
二、UI开发流程
1. UI自动导出对应代码文件，后面修改预制体后导出自动替换对应文件的代码，快捷键为F1，可参考UIBind.cs中的代码，在CollectSetting中根据自己习惯自定义导出配置,格式类似于Game_Button。
2. UI框架使用方法，比如新增LoginWindow.cs界面文件，生成的UI代码定义有预制体的路径，支持"UIRoot.Inst.ShowWindow<LoginWindow>(自定义参数);"这种快捷方式打开UI，
   自动加载，同时也会处理ModalLayer这种放在UI下面的半透明黑色背景的层级。在界面打开后自动绑定事件，关闭界面时自动解绑事件。同时也可以在打开和关闭界面时添加对应的动画
3. 附带了Unity商店中的EnhancedScroller，支持自定义高度以及虚拟化列表项，可无限滚动的列表组件。仅供学习，请支持正版
TODO：
1. 一键热更，打包，可配合Jenkins实现自动化
