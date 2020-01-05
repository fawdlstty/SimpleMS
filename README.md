# SimpleMS

.Net Core 3.1 下极简的微服务框架

## 项目特点

最大的区别是，注册中心作为扩展功能嵌入网关，极大简化架构，并使得新手更容易上手

## 快速教程

下面的单体教程对于有基础的开发者来说可以快速上手；另外虽然提供了完整的示例，但通过这个例子可以更细致的说明哪些点应该怎样设计，及这么设计的原因

我们一般开发后台服务是对外提供 REST API 接口，我们现在使用这个框架来动手开发一个单体应用：

新建 ASP.NET Core Web 应用程序，类型选API，并引用项目 `Fawdlstty.SimpleMS`

修改 `Startup.cs` 为：

```csharp
public void ConfigureServices (IServiceCollection services) {
    // 任何使用此框架功能的项目都需要使用这一句引用 SimpleMS
    services.AddSimpleMS ((_option) => {
        // 指定本项目 `ASP.Net Core` 对外提供服务的端口
        // 只要是有网关功能，或者需要对外提供服务，那么必需；如果只是服务调用者，没有网关或服务提供，那么不要指定
        _option.LocalPort = 5000;
        // 指定服务发现方式为注册中心方式，如果没有上级网关那么不需要再指定参数
        _option.SetRegCenterDiscovery (TimeSpan.FromSeconds (10), TimeSpan.FromSeconds (1));
    });
}

public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
    // 单体应用或者微服务网关需要使用网关路由
    // 如果不指定前缀，那么默认 `/api`
    app.UseSimpleMSGateway ();
}
```

好了，下面我们再来开发对外提供的服务的声明，此处的服务的作用类似 MVC 的 Controller（单体应用可以暂时不用拆分为其他项目）：

```csharp
// 服务提供者接口必须具有这个标注
[SimpleMSService]
public interface IMyService {
    // 返回类型必须是 Task 类型或 Task<T> 类型
    Task<string> Hello ();
}

// 服务提供者类，必须具有无参数的构造函数
// 一个进程单位里面不要有两个服务提供者类同时继承自同一个服务提供者接口
public class MyService: IMyService {
    public Task<string> Hello () {
        return Task.FromResult ("hello test service");
    }
}
```

OKOK，此时单体应用已经搭建完毕，启动后浏览器访问 <http://127.0.0.1:5000/api?module=TestGateway.IMyService&method=Hello> 即可看到效果。此处有两个参数，module 含义为需要调用的服务提供者类的 `FullName`，method 含义为指定的服务提供者类的具体函数名。

现在单体服务搭建完毕，我们来拆分微服务。首先新建接口说明项目，将 IMyService 移进去

然后新建服务提供项目，引用接口项目并实现 IMyService，然后修改 `Startup.cs` 为：

```csharp
public void ConfigureServices (IServiceCollection services) {
    services.AddSimpleMS ((_option) => {
        _option.LocalPort = 5001;
        // 注册中心方式的服务发现必须调用
        _option.SetRegCenterDiscovery (TimeSpan.FromSeconds (10), TimeSpan.FromSeconds (1), ("127.0.0.1", 5000));
    });
}

public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
    // 作为服务模块对外提供服务那么就是用这个，如果既是网关也对外提供服务（比如对外接上级网关与鉴权服务，对内接细分服务）那么需要两者都调用一下
    app.UseSimpleMSService ();
}
```

然后移除网关的这两个模块。并运行网关与服务提供者，过一会就会发现网关控制台打印出 `Service 127.0.0.1:5001 online.`，代表服务提供者正确的在网关上注册成功。这时候可以按照之前调用网关上的服务的方式调用外部服务。

另外微服务部署通常有多个实例，可以把服务提供者项目复制一份，端口改为5002，然后同时运行，网关控制台会打印两条在线信息，这时候按照以前方式请求，不停刷新，就会发现两个服务被轮询调用。

说完了网关，再说说客户端调用。这个使用方式稍微有点区别。我们现在在一个Web项目里面做网关调用示例。首先新建客户端项目，然后引用接口，并修改 `Startup.cs` 为：

```csharp
public void ConfigureServices (IServiceCollection services) {
    // 其他的不用修改，在最前面添加一下
    services.AddSimpleMS ((_option) => {
        //由于我们没有提供网关服务或服务提供者，所以不用指定本地服务端口
        // 注册中心方式的服务发现必须调用
        _option.SetRegCenterDiscovery (TimeSpan.FromSeconds (10), TimeSpan.FromSeconds (1), ("127.0.0.1", 5000));
    });
    services.AddControllers ();
}

// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
	// 由于我们没有提供网关服务或服务提供者，所以不用添加或删除什么，框架自动生成的其他代码均省略，不代表删除
}
```

好了，现在我们新建一个控制器，并在控制器里面使用客户端功能（也就是调用服务）：

```csharp
public class TestController: ControllerBase {
	private readonly IMyService _service;
	public TestController (IMyService service) {
		_service = service;
	}

	[HttpGet]
	public async Task<string> index () {
		try {
			return await _service.Hello ();
		} catch (Exception ex) {
			return ex.ToString ();
		}
	}
}
```

需要说明两点，一点是，服务全部提供了，可以在需要时直接注入，注意此处注入的服务全部是单例；还有一点是，如果有异常，比如服务提供者没有查询到，或者返回错误，那么会直接抛出异常，需要手工捕获。

这时候一直刷新调用这个接口，就会发现轮询调用了两个服务提供者。假如一个服务提供者下线了，那么过十几秒，然后再一直刷新，就会发现请求的全部是在线的服务了。

另外框架提供自定义服务发现的方式的支持，比如DNS、环境变量服务发现等，只需要将上面 `_option.SetRegCenterDiscovery` 这一行改为：

```csharp
// 自定义服务发现，根据服务名称获取服务地址及端口，参数有两种格式
// 示例服务名称：“ExampleProject.IMyService:a385b0b48e760ed8e8167e24141fbbe4”、“ExampleProject.IMyService:”
//     ExampleProject 为服务接口的命名空间
//     IMyService 为服务接口名称
//     a385b0b48e760ed8e8167e24141fbbe4 为服务hash。如果有增删改方法或参数、参数顺序有调整，那么这个值会改变，也可能没有这个值
// 此处处理是DNS服务发现带环境变量服务发现的方式，需根据实际情况做修改
_option.SetCustomDiscovery ((_service_name) => {
    // 将格式化参数为 EXAMPLEPROJECT_IMYSERVICE
    _service_name = _service_name.Substring (0, _service_name.IndexOf (':')).ToUpper ().Replace (".", "_");
    // 查询服务地址，此处环境变量格式为“地址:端口”，并指定没查到的情况（地址为空，端口为0）
    string _ret = Environment.GetEnvironmentVariable (_service_name) ?? ":0";
    int _split = _ret.IndexOf (':');
    // 返回服务地址与端口
    return (_ret.Substring (0, _split), int.Parse (_ret.Substring (_split + 1)));
});
```

## TODO

1. 网关提供 swagger 文档
2. 鉴权
