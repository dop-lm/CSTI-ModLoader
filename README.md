# CSTI-ModLoader

CSTI-ModLoader是卡片生存游戏Card Survival: Tropical Island的一个通用Mod载入器，能够自动载入按照一定规则编写的Json格式的Mod数据，还能载入自定义的贴图等资源

`MOD开发交流QQ群号：641891277`

## 依赖

### Card Survival: Tropical Island游戏

### BepInEx 5 x64

可以通过前往<https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21>下载，下载完成后解压到游戏根目录

## 安装

将下载的压缩文件解压后，把`ModLoader.dll`文件放到`BepInEx/plugins`底下，最终的目录结构如下

```
<Steam Directory>\steamapps\common\Card Survival Tropical Island
└───BepInEx
│   └───plugins
│   │   │   ModLoader.dll
│   │   │   ...   
│   ...
...
```

# Json-Mod编写

详见BaseMod（Json-Mod示例）内的说明文件


# 更新计划

* 完善GameStat（游戏状态）和PlayerCharacter（游戏角色）的自定义功能
* 增加对已有卡片的修改功能
* 编写GUI的JSON-MOD编辑工具，准备起名叫CSTI-ModEditor
* 增加自定义音效

# 分支说明
因为dop并未使用nstrip或类似工具避免反射，所以单独开了个分支
引用dll的路径也有所不同