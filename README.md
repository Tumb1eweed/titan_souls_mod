# Titan Souls ESP Mod

显示 Boss、角色和箭的位置。

## 功能

- 实时显示玩家位置
- 显示 Boss 位置和血量
- 追踪射出的箭

## 使用方法

1. 安装 [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
2. 用 Visual Studio 打开 `TitanSoulsESP.csproj`
3. 还原 NuGet 包 (Memory.dll)
4. 编译运行
5. 启动 Titan Souls 游戏
6. ESP 会自动附加到游戏进程

## 找偏移地址

当前代码中的偏移是占位符，需要用 [Cheat Engine](https://www.cheatengine.org/) 找到实际地址:

### 步骤

1. 打开游戏和 Cheat Engine
2. 附加到 TITAN.exe 进程
3. 扫描玩家坐标 (移动后搜索变化的值)
4. 找到基址和偏移
5. 更新 `GameReader.cs` 中的偏移

### 需要找的地址

| 数据 | 描述 |
|------|------|
| 玩家 X | X 坐标 |
| 玩家 Y | Y 坐标 |
| 玩家血量 | 当前血量 |
| 是否有箭 | 箭是否在手中 |
| 箭 X/Y | 箭的坐标 |
| Boss 列表基址 | Boss 数组指针 |
| Boss 数量 | 当前 boss 数量 |

## 文件结构

```
├── Program.cs        # 入口点
├── MainForm.cs       # Overlay 窗体
├── MemoryManager.cs  # 内存读取
├── GameReader.cs     # 游戏数据解析
└── GameData.cs       # 数据结构
```
