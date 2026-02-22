# Cheat Engine 偏移查找指南

## 基础概念

- **基址 (Base Address)**: 模块加载地址，每次启动相同
- **偏移 (Offset)**: 从基址到目标数据的距离
- **指针链**: 多级指针 `[[基址 + 偏移1] + 偏移2] -> 数据`

## 准备工作

1. 下载 [Cheat Engine](https://www.cheatengine.org/)
2. 以管理员身份运行 CE
3. 启动 Titan Souls 游戏

---

## 1. 找玩家坐标

### 步骤

1. **附加进程**: CE 左上角电脑图标 → 选择 `TITAN.exe`

2. **首次扫描**:
   - Value type: `Float` (坐标通常是浮点数)
   - Scan type: `Unknown initial value`
   - 点击 First Scan

3. **移动角色**:
   - 在游戏中向右移动一段距离
   - 回到 CE, Scan type 选 `Increased value`
   - 点击 Next Scan

4. **反向移动**:
   - 在游戏中向左移动
   - CE 选 `Decreased value`
   - Next Scan

5. **重复筛选**:
   - 继续移动并筛选，直到剩下少量地址
   - 双击地址加入列表

6. **确认 X 坐标**:
   - 右键地址 → Browse this memory region
   - 观察: X 坐标附近 (±0x10) 通常有 Y 坐标

```
内存布局示例:
0x10: X 坐标 (float)
0x14: Y 坐标 (float)
0x20: 血量 (float or int)
```

---

## 2. 找指针链 (重要!)

直接找到的动态地址重启后会变，需要找指针:

1. 右键找到的地址 → `Pointer scan this address`

2. 或者手动找指针:
   - 右键地址 → `Find out what accesses this address`
   - 在游戏中移动
   - 观察汇编代码，找到类似:
   ```asm
   mov eax, [ebx + 10]  ; ebx 是基址, 10 是偏移
   ```

3. 扫描指针:
   - 新扫描 → Value type: `4 Bytes`
   - 勾选 `Hex`
   - 输入找到的指针值
   - 找到稳定的基址

---

## 3. 找血量

1. **首次扫描**: Value type 选 `Float` 或 `4 Bytes`
2. **受伤**: 在游戏中让 boss 打你
3. **扫描**: `Decreased value`
4. **回血** (如果有): `Increased value`
5. **重复**直到找到

---

## 4. 找 Boss 数据

### 方法 A: 扫描变化值

1. 进入有 boss 的房间
2. 扫描 `Unknown initial value`
3. 离开房间 → `Changed value`
4. 重新进入 → `Changed value`
5. 重复筛选

### 方法 B: 扫描数量

1. 在 boss 房间
2. 如果知道 boss 血量数值，直接扫描该值
3. 打 boss → 扫描 `Decreased value`

---

## 5. 找箭的位置

1. **持箭状态**:
   - 扫描 `1` (byte)
   - 射出箭
   - 扫描 `0`
   - 回收箭
   - 扫描 `1`

2. **箭坐标**:
   - 射出箭
   - 用找坐标的方法找箭的 X/Y
   - 箭通常有独立的数据结构

---

## 6. 实用技巧

### 观察数据结构

```
右键地址 → Browse this memory region

常见布局:
+0x00: 指针/标识
+0x04: 指针/标识  
+0x08: 指针/标识
+0x0C: 实体类型
+0x10: X 坐标
+0x14: Y 坐标
+0x18: Z 坐标 (如果是3D)
+0x1C: 旋转角度
+0x20: 血量
+0x24: 最大血量
+0x28: 速度 X
+0x2C: 速度 Y
+0x30: 状态标志
```

### 验证偏移

在 CE 中:
```
地址表达式: "TITAN.exe"+基址偏移+指针偏移
例如: "TITAN.exe"+00ABC123+10
```

如果重启游戏后地址仍有效，说明找对了。

---

## 7. 更新代码

找到偏移后，更新 `GameReader.cs`:

```csharp
// 示例 (替换为你找到的实际值)
private const int PLAYER_BASE_OFFSET = 0x123456;  // 替换
private const int PLAYER_X_OFFSET = 0x10;
private const int PLAYER_Y_OFFSET = 0x14;
private const int PLAYER_HEALTH_OFFSET = 0x20;
private const int PLAYER_HAS_ARROW_OFFSET = 0x30;
private const int ARROW_X_OFFSET = 0x40;
private const int ARROW_Y_OFFSET = 0x44;
```

---

## 常见问题

**Q: 地址每次都变怎么办?**
A: 需要找指针链，或者扫描静态地址

**Q: 找不到稳定的基址?**
A: 尝试扫描 `TITAN.exe` 模块内的偏移，用 `module.base + offset`

**Q: 游戏是 GameMaker 引擎?**
A: Titan Souls 用 GameMaker，数据结构可能有特定模式。查找:
- 全局变量表
- 实例列表
- `global.player` 或类似结构
