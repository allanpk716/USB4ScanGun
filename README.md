USB4ScanGun
============


PS：最近在测试点东西，要看这个项目的朋友麻烦用稍早前的版本。之后测试结果也会写在这里，或者博客中的。--2014年9月28日


USB扫描枪识别，已获取对应扫描枪返回值的测试程序。

1，部分代码来源网络，本人只是测试、搬运而已；

2，请使用 VS2010 以上的版本打开；

3，请主要参考 Demo 这个项目，其他都是测试的；

程序主要使用到了 Win 中的 RawInput 来获取硬件信息，这里请参考 MSDN 的解释。

这里大致说一下：

1，RawInput 必须在 WM_INPUT 事件除获取，这里也是 MSDN 提到的；

2，项目中的 RawInput 这个工程，接收事件是 Win Form 程序，对于 keyBoard 这项目，是 WPF 直接调用 RawInput 工程来做测试的，但是具体的消息打算自己处理，所以 Demo 由此而来，当然仅仅是改写为 WPF 来使用，期间会遇到一些问题；

Ps：下面都是说 Demo

3，原有在 WndProc 这个回调截取消息的，但是这里获取的时候就太晚了，所以在 Demo 中会用到另一个回调函数（ComponentDispatcher_ThreadFilterMessage）；

Ps：关于 WPF 中 按键消息的传递顺序， ThreadFilterMessage > PreKeyDown > WndProc，实际上当你在 WndProc 中屏蔽消息的时候，其实已经刷新到响应的控件上了。

4，由于 3 的缘故，导致 在这里无法响应 WM_DEVICECHANGE 这个消息，所以后面我还是用了 WndProc 这个回调函数来做硬件拔插的事件；

5，进过测试，在 WM_INPUT 处，handle = true 是无法屏蔽消息的，必须要在 WM_KEYDOWN 处进行屏蔽，但是···在 WM_KEYDOWN 的时候是无法获取 RawInput 的硬件信息的。

6，其实我也没有能做到屏蔽指定硬件的输入信息，而是变相的做，其实很简单，在 ComponentDispatcher_ThreadFilterMessage 这里将窗体的焦点弄到一个 Labl 或者 其他静态无输入事件的控件上，然后这里会检测 扫描枪 的一个回车结束事件，这然就认为一次扫描完毕，然后之前的信息都存在一个队列中，之后就是弄出来，拼接好传到界面上显示就好了；

7，对于 Demo 来说，首先你要标定扫描枪，也就是在 子界面上获取一次硬件信息，之后才能在 主界面上响应对应的逻辑；

8，其实 在 Demo 的子界面上没有必要再实例化一次 RawInput 的，其实直接 添加一次消息获取就好了，System.Windows.Interop.ComponentDispatcher.ThreadFilterMessage +=
                    new System.Windows.Interop.ThreadMessageEventHandler(ComponentDispatcher_ThreadFilterMessage);

9， 8 其实是后话，各位可以自己实践。

10，对应过滤这里其实一直是诟病，目前在下是没找到 R3 层面上更好的办法来解决了，看到的是 R0 层面的，还是算了，如果有朋友能解决这个问题，请果断 push 上来！或者发邮件联系我！先谢了！

Good luck！

