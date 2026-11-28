# SCRAP NATION — Humanoid corrective sources V02

Date: 2026-11-28. Source baseline: main at `f21abae1d06dd7fa3a3d2116384ed32de588e2b7` (merged PR #7). All images are original ChatGPT Images outputs, unchanged byte-for-byte. This package is an additive source delivery, not a ready-to-import rig. It does not replace any V01 file. The re-uploaded V01 references and temporary QA contact sheet are excluded.

- Only Legs V02 has explicit visual approval. Other primary sheets are selected candidates; independent modules, face matching, overlaps and rig fit remain unverified. Additional candidates are optional; archived images are not production inputs.
- Preserve the V01 manifest, source catalog, `.meta`, prefab, clips and reproducible evidence. Extract `ArtSource/` into the repository root, not into Unity Assets. Append a link to this catalog in the existing `ArtSource/README.md` after publication. Verify both manifests from the ArtSource directory; the V02 manifest covers only these 17 new PNGs.
- Actual alpha histograms below. Transparent channels do not guarantee clean background. Especially Legs V02: 45.58% of the entire image has alpha 1–12. Inspect visible edges on light and dark backgrounds, preserve anti-aliasing; do not automatically treat every faint pixel as contamination. Never edit originals.
- Work on copies using the V01 rig and 543 × 724 coordinate frame (`Tools/HumanoidAssemblyV01/layout.json`, 360 PPU). Initial overlap hypotheses: head 8–12 px; shoulder/elbow 10–14 px; knee 12–18 px, in that frame. Do not claim exact generated anchors. Read the existing benchmark and reuse its measured joints. The selected art still needs redrawing/registration if hidden geometry is missing.
- For head registration, preserve V01 visible face pixels wherever practical. If selected bald face differs, use reference-preserving masking/paint-over; leave V01 combined head as explicitly marked fallback if exact identity cannot be preserved. No silent replacement with a different person.
- For the jacket, independent scarf swapping is mandatory for a successful V02 test. The latest jacket contains a baked scarf; remove on a derivative or select a compatible scarf-free alternative with separate sleeves. Do not fake a successful scarf-off test by hiding only the independent scarf.
- The infographic images in Archive are concepts, not measurements or screenshots of V02.

## Inventory

Paths in this table are relative to ArtSource. Status and crop notes are also in `Characters/Humanoid/Generated/V02/Inventory.csv`.

| Path | Status | Dimensions | Alpha = 0 | Alpha 1–12 | Notes |
| --- | --- | --- | ---: | ---: | --- |
| `Characters/Humanoid/Generated/V02/SN_Humanoid_Legs_V02.png` | Approved visual source | 1448 × 1086 RGBA | 32.16% | 45.58% | 8 parts, top row Near, bottom Far; thigh, shin, foot, boot. Strong residual alpha; rounded end surfaces still need seam and boot fit QA. |
| `Characters/Humanoid/Generated/V02/SN_Humanoid_Clothing_V02.png` | Candidate | 1536 × 1024 RGBA | 55.02% | 0.88% | Latest short-sleeve jacket sheet: torso left, Near sleeve middle, Far sleeve right. Integral red scarf and fleece collar remain; isolate scarf or use alternative torso. Confirm cuffs end above elbow. |
| `Characters/Humanoid/Generated/V02/SN_Humanoid_Head_V02.png` | Candidate | 1536 × 1024 RGBA | 50.92% | 8.42% | Top row first head, second row first hair, third row first round goggles are proposed 3/4 right crops. Verify identity and register to V01; not exact layers. Neck in head must be separated. |
| `Characters/Humanoid/Generated/V02/SN_Humanoid_Arms_V02.png` | Candidate | 1536 × 1024 RGBA | 55.28% | 5.26% | First two columns only: top upper arms, middle forearms, bottom hands. Confirm Near/Far and wrist/elbow fit. Other clothing variants excluded from base anatomy. |
| `Characters/Humanoid/Generated/V02/Additional/SN_Humanoid_Trousers_V01.png` | Additional candidate | 1536 × 1024 RGBA | 81.16% | 1.78% | Clothed thighs/shins, feet and boots; not bare anatomy. Pants compatibility untested. |
| `Characters/Humanoid/Generated/V02/Additional/SN_Humanoid_EquipmentAtlas_V02_01.png` | Additional candidate | 1536 × 1024 RGBA | 23.65% | 27.10% | General atlas with head, hair, goggles, vest without scarf and rolled sleeves; optional alternative torso; many merged modules. |
| `Characters/Humanoid/Generated/V02/Additional/SN_Humanoid_EquipmentAtlas_V02_02.png` | Additional candidate | 1536 × 1024 RGBA | 38.65% | 11.20% | General atlas with hair/goggles, vest and sleeves, trousers and pouches; use only specific compatible crops. |
| `Archive/Characters/V02/SN_Concept_Jacket_LongSleeves_01.png` | Archive | 1536 × 1024 RGBA | 56.49% | 1.18% | Long sleeves and integral scarf. |
| `Archive/Characters/V02/SN_Concept_Jacket_LongSleeves_02.png` | Archive | 1536 × 1024 RGBA | 57.51% | 1.81% | Long sleeves, alternative yellow jacket. |
| `Archive/Characters/V02/SN_Concept_Jacket_LongSleeves_03.png` | Archive | 1536 × 1024 RGBA | 56.40% | 1.44% | Long sleeves and integral scarf. |
| `Archive/Characters/V02/SN_Concept_Jacket_ShortSleeves_01.png` | Archive | 1536 × 1024 RGBA | 60.24% | 0.79% | Short sleeves but integral scarf and changed jacket design. |
| `Archive/Characters/V02/SN_Concept_Jacket_ShortSleeves_02.png` | Archive | 1536 × 1024 RGBA | 57.21% | 0.66% | Short sleeve variant; integral scarf. |
| `Archive/Characters/V02/SN_Concept_HeadAtlas_01.png` | Archive | 1536 × 1024 RGBA | 3.95% | 10.76% | Different face and extra rotations. |
| `Archive/Characters/V02/SN_Concept_ArmsEquipment_01.png` | Archive | 1536 × 1024 RGBA | 59.09% | 3.60% | Missing bare upper arms, redundant equipment variants. |
| `Archive/Characters/V02/SN_Concept_V02_ProductionSheet_01.png` | Archive | 1536 × 1024 RGBA | 0.00% | 0.00% | Generated infographic, not evidence of technical validation. |
| `Archive/Characters/V02/SN_Concept_V02_ProductionSheet_02.png` | Archive | 1536 × 1024 RGBA | 0.00% | 0.00% | Generated infographic, not evidence of technical validation. |
| `Archive/Characters/V02/SN_Concept_V02_ProductionSheet_03.png` | Archive | 1536 × 1024 RGBA | 0.00% | 0.00% | Generated infographic, not evidence of technical validation. |

## Validation

All 17 PNGs decoded fully; all hashes are distinct; all lack PNG metadata fields. Verify after extraction from ArtSource with `sha256sum -c SHA256SUMS_V02.txt` or an equivalent script. Unity validation is NOT TESTED for V02.
