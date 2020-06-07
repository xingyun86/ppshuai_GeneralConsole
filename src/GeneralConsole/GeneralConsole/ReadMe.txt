====================================

GeneralConsole使用说明

====================================

支持操作系统:Windows/Linux/Mac

1.GeneralConsole启动配置config.json
{
  "IsServer": 1,//,0-客户端, 1-服务器
  "Host": "0.0.0.0",//0-客户端时要连接的服务器地址
  "Port": 18081//端口
}
2.GeneralConsole启动参数
GeneralConsole //默认读取config.json
GeneralConsole client.json//读取client.json启动客户端
GeneralConsole server.json//读取server.json启动服务器
