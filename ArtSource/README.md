# SCRAP NATION — ArtSource V01

Исходники от 2026-11-27. Размещаются вне Unity Assets/; в игру попадают только обработанные и проверенные спрайты. Утверждение внешности не означает утверждения rig или совместимости модулей.

В каталоге опубликованы 12 PNG и один SVG. Все 13 оригиналов прошли проверку по [манифесту SHA-256](SHA256SUMS.txt).

## Утверждённые референсы

| Файл | Назначение |
| --- | --- |
| [SN_Concept_Scene_01.png](References/Approved/World/SN_Concept_Scene_01.png) | Утверждено |
| [SN_Hero_Base_V01.png](References/Approved/Characters/SN_Hero_Base_V01.png) | Утверждено |
| [SN_Humanoid_Anatomy_V01.png](References/Approved/Characters/SN_Humanoid_Anatomy_V01.png) | Утверждено визуально |

## Экспериментальные листы гуманоида

| Файл | Назначение |
| --- | --- |
| [SN_Humanoid_Core_V01.png](Characters/Humanoid/Generated/SN_Humanoid_Core_V01.png) | Исходник A |
| [SN_Humanoid_Arms_V01.png](Characters/Humanoid/Generated/SN_Humanoid_Arms_V01.png) | Исходник B |
| [SN_Humanoid_Legs_V01.png](Characters/Humanoid/Generated/SN_Humanoid_Legs_V01.png) | Исходник C |

## Архив концептов

| Файл | Назначение |
| --- | --- |
| [SN_Concept_Scene_01_CompositionStudy.png](Archive/World/SN_Concept_Scene_01_CompositionStudy.png) | Заменённый программный макет |
| [SN_Concept_Scene_01_CompositionStudy.svg](Archive/World/SN_Concept_Scene_01_CompositionStudy.svg) | Исходник макета |
| [SN_Concept_CharacterEquipment_01.png](Archive/Characters/SN_Concept_CharacterEquipment_01.png) | Дополнительный концепт экипировки |
| [SN_Concept_ArmsCatalog_01.png](Archive/Characters/SN_Concept_ArmsCatalog_01.png) | Презентационный лист, отклонён как атлас |
| [SN_Concept_ArmsCatalog_02.png](Archive/Characters/SN_Concept_ArmsCatalog_02.png) | Презентационный лист, отклонён как атлас |
| [SN_Concept_LegsCatalog_01.png](Archive/Characters/SN_Concept_LegsCatalog_01.png) | Презентационный лист, отклонён как атлас |
| [SN_Concept_LegsCatalog_02.png](Archive/Characters/SN_Concept_LegsCatalog_02.png) | Презентационный лист, отклонён как атлас |

## Проверка исходных файлов

Изображения созданы ChatGPT Images, кроме программного композиционного макета. Дубликат сцены и производные QA-превью исключены. В каталоге сохранены исходные байты, без пересжатия или обработки. Оригиналы не имеют встроенных PNG metadata-полей по результату чтения Pillow; SVG не содержит обнаруженных локальных путей или внешних ссылок. Не добавлять сторонние материалы без права публикации.

Сцена: 1672 × 941 RGB. Герой и анатомия: 1086 × 1448 RGBA. Листы A/B/C: 1536 × 1024 RGBA; альфа в диапазоне 0–254, полностью прозрачные пиксели составляют соответственно 45,85%, 85,09%, 74,80%. Слабопрозрачные остатки и непрозрачность внутренних областей требуют проверки и обработки. Проверка файлов не заменяет Unity validation.

Контрольные суммы всех оригиналов — [SHA256SUMS.txt](SHA256SUMS.txt). После копирования из каталога ArtSource выполнить `sha256sum -c SHA256SUMS.txt` (или эквивалент в ОС). Не подменять исходники превью, очищенными производными или LFS-указателями без объекта. См. [Art Direction](../Docs/GameDesign/ArtDirection.md).

## Humanoid Assembly V02

[Каталог 17 оригиналов V02](Characters/Humanoid/Generated/V02/README.md) · [Инвентаризация](Characters/Humanoid/Generated/V02/Inventory.csv) · [SHA-256](SHA256SUMS_V02.txt).
