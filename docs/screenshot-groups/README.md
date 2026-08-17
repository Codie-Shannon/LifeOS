# Screenshot-group identifier lineage

Screenshot-group identifiers are aligned to the ending implementation group of
the evidence lane. They are checkpoint identifiers, not a separate sequential
counter.

Groups 1-66 were already effectively one-to-one. From Group 67 onward, several
implementation groups were compressed into each evidence pack. The repository
previously continued incrementing a separate SG counter, which made current
work appear to stop at SG-82 even though implementation had reached Group 128.

## Corrected checkpoint mapping

| Implementation groups | Correct SG |
|---|---:|
| 67-68 | SG-68 |
| 69-72 | SG-72 |
| 73-76 | SG-76 |
| 77-79 | SG-79 |
| 80-82 | SG-82 |
| 83-86 | SG-86 |
| 87-90 | SG-90 |
| 91-94 | SG-94 |
| 95-98 | SG-98 |
| 99-103 | SG-103 |
| 104-107 | SG-107 |
| 108-111 | SG-111 |
| 112-116 | SG-116 |
| 117-120 | SG-120 |
| 121-124 | SG-124 |
| 125-128 | SG-128 |
| 129-132 | SG-132 |
| 133-136 | SG-136 |
| 137-140 | SG-140 |
| 141-144 | SG-144 |
| 145-148 | SG-148 |
| 149-152 | SG-152 |
| 153-156 | SG-156 |
| 157-160 | SG-160 |
| 161-164 | SG-164 |

## Evidence preservation

Historical screenshots are not edited. Some captured UI or terminal text still
shows the former sequential label because that is what the application rendered
when the evidence was taken. Folder manifests, validation-asset filenames and
current product code use the corrected checkpoint identifiers. This preserves
the original pixels while making the lineage accurate going forward.

SG-128 Pack 1 is implemented and its visible Desktop capture remains open.
SG-132 is closed with the exact eight-image Desktop and Full Mobile Pack 2
evidence set.

SG-136 is closed with the exact eight-image Desktop and validation Pack 2
evidence set.

SG-140 is closed with the exact eight-image Desktop and validation Pack 2
evidence set.

SG-144 is closed with the exact eight-image Desktop and validation Pack 2
evidence set.

SG-148 is closed with the exact eight-image Desktop and validation Pack 2
evidence set.

SG-152 is closed with the exact eight-image Desktop and validation Pack 2
evidence set.

SG-156 is closed with the exact eight-image Desktop and validation Pack 2
evidence set.

SG-160 is closed with the exact eight-image Desktop and validation Pack 2
evidence set.

SG-164 Pack 1 is implemented. Its directly rendered Desktop and validation Pack
2 evidence remains open.
