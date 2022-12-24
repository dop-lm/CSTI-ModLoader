# Json-Mod

## Modinfo.json

只有带有`Modinfo.json`的文件夹会被视为是一个Json-Mod，ModLoader才会去导入这个Mod，这里面可以放一些Mod名字、版本号、描述等信息

## CardData|CharacterPerk|GameStat|Objective|SelfTriggeredAction

存放对应类型的数据，比如要建一个新的CardData类型，如果新建的名字叫NewCard，那么最终的目录结构如下

```
CardData
└───NewCard
│   │   NewCard.json 
│   │   ...
│   ...
...
```

NewCard.json为新建CardData的基础数据，建议先从游戏原始Json数据中找一个最类似的拷贝过来改名字，比如做蓝图类就找蓝图类的，做工兵铲就去找铜铲

## Localization

本地化文件的存放文件夹，目前只支持中文，将新建文件中所有不为空的LocalizationKey拷贝到一个名为SimpCn.csv的文件下，然后添加中文翻译

格式为`LocalizationKey,原文，翻译`注意逗号位置，如果原文中有逗号，则需要改成`LocalizationKey,"原文"，翻译`

## Resource

自定义资源的存放文件夹，ModLoader会加载UnityEditor打包的AssetBundle资源文件，需要以".ab"结尾，其中的音效和图片都会被载入，也可以使用自定义的jpg、jpeg、png格式的图片，只需要对应图片放到Resource\Picture文件夹底下