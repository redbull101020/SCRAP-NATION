# SCRAP NATION — ArtSource V01

Исходники от 2026-11-27. Размещаются вне Unity Assets/; в игру попадают только обработанные и проверенные спрайты. Утверждение внешности не означает утверждения rig или совместимости модулей.

**Статус публикации:** 12 PNG и один SVG отсортированы и сохранены побайтно в локально подготовленном архиве `SCRAP_NATION_ArtSource_V01.zip`. На момент этого документационного PR оригиналы ещё не загружены в GitHub: коннектор не обеспечивает передачу исходных бинарных файлов из чата. Ниже целевые пути, а не ссылки на уже существующие файлы. После размещения оригиналов проверить контрольные суммы и обновить статус.

| Целевой путь относительно ArtSource/ | Статус |
| --- | --- |
| `References/Approved/World/SN_Concept_Scene_01.png` | Утверждено |
| `References/Approved/Characters/SN_Hero_Base_V01.png` | Утверждено |
| `References/Approved/Characters/SN_Humanoid_Anatomy_V01.png` | Утверждено визуально |
| `Characters/Humanoid/Generated/SN_Humanoid_Core_V01.png` | Исходник A |
| `Characters/Humanoid/Generated/SN_Humanoid_Arms_V01.png` | Исходник B |
| `Characters/Humanoid/Generated/SN_Humanoid_Legs_V01.png` | Исходник C |
| `Archive/World/SN_Concept_Scene_01_CompositionStudy.png` | Заменённый программный макет |
| `Archive/World/SN_Concept_Scene_01_CompositionStudy.svg` | Исходник макета |
| `Archive/Characters/SN_Concept_CharacterEquipment_01.png` | Дополнительный концепт экипировки |
| `Archive/Characters/SN_Concept_ArmsCatalog_01.png` | Презентационный лист, отклонён как атлас |
| `Archive/Characters/SN_Concept_ArmsCatalog_02.png` | Презентационный лист, отклонён как атлас |
| `Archive/Characters/SN_Concept_LegsCatalog_01.png` | Презентационный лист, отклонён как атлас |
| `Archive/Characters/SN_Concept_LegsCatalog_02.png` | Презентационный лист, отклонён как атлас |

## Проверка исходных файлов

Изображения созданы ChatGPT Images, кроме программного композиционного макета. Дубликат сцены и производные QA-превью исключены. В архиве содержатся исходные байты, без пересжатия или обработки. Оригиналы не имеют встроенных PNG metadata-полей по результату чтения Pillow; SVG не содержит обнаруженных локальных путей или внешних ссылок. Не добавлять сторонние материалы без права публикации.

Сцена: 1672 × 941 RGB. Герой и анатомия: 1086 × 1448 RGBA. Листы A/B/C: 1536 × 1024 RGBA; альфа в диапазоне 0–254, полностью прозрачные пиксели составляют соответственно 45,85%, 85,09%, 74,80%. Слабопрозрачные остатки и непрозрачность внутренних областей требуют проверки и обработки. Проверка файлов не заменяет Unity validation.

Ожидаемые контрольные суммы всех оригиналов — [SHA256SUMS.txt](SHA256SUMS.txt). После копирования из каталога ArtSource выполнить `sha256sum -c SHA256SUMS.txt` (или эквивалент в ОС). Не подменять исходники превью, очищенными производными или LFS-указателями без объекта. См. [Art Direction](../Docs/GameDesign/ArtDirection.md).
