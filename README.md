# SimpleMS

.Net Core 3.1 �¼����΢������

## ��Ŀ�ص�

���������ǣ�ע��������Ϊ��չ����Ƕ�����أ�����򻯼ܹ�����ʹ�����ָ���������

## ���ٽ̳�

����ĵ���̶̳����л����Ŀ�������˵���Կ������֣�������Ȼ�ṩ��������ʾ������ͨ��������ӿ��Ը�ϸ�µ�˵����Щ��Ӧ��������ƣ�����ô��Ƶ�ԭ��

����һ�㿪����̨�����Ƕ����ṩ REST API �ӿڣ���������ʹ�������������ֿ���һ������Ӧ�ã�

�½� ASP.NET Core Web Ӧ�ó�������ѡAPI����������Ŀ `Fawdlstty.SimpleMS`

�޸� `Startup.cs` Ϊ��

```csharp
public void ConfigureServices (IServiceCollection services) {
    // �κ�ʹ�ô˿�ܹ��ܵ���Ŀ����Ҫʹ����һ������ SimpleMS
    services.AddSimpleMS ((_option) => {
        // ָ������Ŀ `ASP.Net Core` �����ṩ����Ķ˿�
        // ֻҪ�������ع��ܣ�������Ҫ�����ṩ������ô���裻���ֻ�Ƿ�������ߣ�û�����ػ�����ṩ����ô��Ҫָ��
        _option.LocalPort = 5000;
        // ָ�������ַ�ʽΪע�����ķ�ʽ�����û���ϼ�������ô����Ҫ��ָ������
        _option.SetRegCenterDiscovery (TimeSpan.FromSeconds (10), TimeSpan.FromSeconds (1));
    });
}

public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
    // ����Ӧ�û���΢����������Ҫʹ������·��
    // �����ָ��ǰ׺����ôĬ�� `/api`
    app.UseSimpleMSGateway ();
}
```

���ˣ����������������������ṩ�ķ�����������˴��ķ������������ MVC �� Controller������Ӧ�ÿ�����ʱ���ò��Ϊ������Ŀ����

```csharp
// �����ṩ�߽ӿڱ�����������ע
[SimpleMSService]
public interface IMyService {
    // �������ͱ����� Task ���ͻ� Task<T> ����
    Task<string> Hello ();
}

// �����ṩ���࣬��������޲����Ĺ��캯��
// һ�����̵�λ���治Ҫ�����������ṩ����ͬʱ�̳���ͬһ�������ṩ�߽ӿ�
public class MyService: IMyService {
    public Task<string> Hello () {
        return Task.FromResult ("hello test service");
    }
}
```

OKOK����ʱ����Ӧ���Ѿ����ϣ���������������� <http://127.0.0.1:5000/api?module=TestGateway.IMyService&method=Hello> ���ɿ���Ч�����˴�������������module ����Ϊ��Ҫ���õķ����ṩ����� `FullName`��method ����Ϊָ���ķ����ṩ����ľ��庯������

���ڵ��������ϣ����������΢���������½��ӿ�˵����Ŀ���� IMyService �ƽ�ȥ

Ȼ���½������ṩ��Ŀ�����ýӿ���Ŀ��ʵ�� IMyService��Ȼ���޸� `Startup.cs` Ϊ��

```csharp
public void ConfigureServices (IServiceCollection services) {
    services.AddSimpleMS ((_option) => {
        _option.LocalPort = 5001;
        // ע�����ķ�ʽ�ķ����ֱ������
        _option.SetRegCenterDiscovery (TimeSpan.FromSeconds (10), TimeSpan.FromSeconds (1), ("127.0.0.1", 5000));
    });
}

public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
    // ��Ϊ����ģ������ṩ������ô����������������������Ҳ�����ṩ���񣨱��������ϼ��������Ȩ���񣬶��ڽ�ϸ�ַ�����ô��Ҫ���߶�����һ��
    app.UseSimpleMSService ();
}
```

Ȼ���Ƴ����ص�������ģ�顣����������������ṩ�ߣ���һ��ͻᷢ�����ؿ���̨��ӡ�� `Service 127.0.0.1:5001 online.`����������ṩ����ȷ����������ע��ɹ�����ʱ����԰���֮ǰ���������ϵķ���ķ�ʽ�����ⲿ����

����΢������ͨ���ж��ʵ�������԰ѷ����ṩ����Ŀ����һ�ݣ��˿ڸ�Ϊ5002��Ȼ��ͬʱ���У����ؿ���̨���ӡ����������Ϣ����ʱ������ǰ��ʽ���󣬲�ͣˢ�£��ͻᷢ������������ѯ���á�

˵�������أ���˵˵�ͻ��˵��á����ʹ�÷�ʽ��΢�е���������������һ��Web��Ŀ���������ص���ʾ���������½��ͻ�����Ŀ��Ȼ�����ýӿڣ����޸� `Startup.cs` Ϊ��

```csharp
public void ConfigureServices (IServiceCollection services) {
    // �����Ĳ����޸ģ�����ǰ�����һ��
    services.AddSimpleMS ((_option) => {
        //��������û���ṩ���ط��������ṩ�ߣ����Բ���ָ�����ط���˿�
        // ע�����ķ�ʽ�ķ����ֱ������
        _option.SetRegCenterDiscovery (TimeSpan.FromSeconds (10), TimeSpan.FromSeconds (1), ("127.0.0.1", 5000));
    });
    services.AddControllers ();
}

// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
	// ��������û���ṩ���ط��������ṩ�ߣ����Բ�����ӻ�ɾ��ʲô������Զ����ɵ����������ʡ�ԣ�������ɾ��
}
```

���ˣ����������½�һ�������������ڿ���������ʹ�ÿͻ��˹��ܣ�Ҳ���ǵ��÷��񣩣�

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

��Ҫ˵�����㣬һ���ǣ�����ȫ���ṩ�ˣ���������Ҫʱֱ��ע�룬ע��˴�ע��ķ���ȫ���ǵ���������һ���ǣ�������쳣����������ṩ��û�в�ѯ�������߷��ش�����ô��ֱ���׳��쳣����Ҫ�ֹ�����

��ʱ��һֱˢ�µ�������ӿڣ��ͻᷢ����ѯ���������������ṩ�ߡ�����һ�������ṩ�������ˣ���ô��ʮ���룬Ȼ����һֱˢ�£��ͻᷢ�������ȫ�������ߵķ����ˡ�

�������ṩ�Զ�������ֵķ�ʽ��֧�֣�����DNS���������������ֵȣ�ֻ��Ҫ������ `_option.SetRegCenterDiscovery` ��һ�и�Ϊ��

```csharp
// �Զ�������֣����ݷ������ƻ�ȡ�����ַ���˿ڣ����������ָ�ʽ
// ʾ���������ƣ���ExampleProject.IMyService:a385b0b48e760ed8e8167e24141fbbe4������ExampleProject.IMyService:��
//     ExampleProject Ϊ����ӿڵ������ռ�
//     IMyService Ϊ����ӿ�����
//     a385b0b48e760ed8e8167e24141fbbe4 Ϊ����hash���������ɾ�ķ��������������˳���е�������ô���ֵ��ı䣬Ҳ����û�����ֵ
// �˴�������DNS�����ִ��������������ֵķ�ʽ�������ʵ��������޸�
_option.SetCustomDiscovery ((_service_name) => {
    // ����ʽ������Ϊ EXAMPLEPROJECT_IMYSERVICE
    _service_name = _service_name.Substring (0, _service_name.IndexOf (':')).ToUpper ().Replace (".", "_");
    // ��ѯ�����ַ���˴�����������ʽΪ����ַ:�˿ڡ�����ָ��û�鵽���������ַΪ�գ��˿�Ϊ0��
    string _ret = Environment.GetEnvironmentVariable (_service_name) ?? ":0";
    int _split = _ret.IndexOf (':');
    // ���ط����ַ��˿�
    return (_ret.Substring (0, _split), int.Parse (_ret.Substring (_split + 1)));
});
```

## TODO

1. �����ṩ swagger �ĵ�
2. ��Ȩ
