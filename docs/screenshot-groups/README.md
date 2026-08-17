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

## Evidence preservation

Historical screenshots are not edited. Some captured UI or terminal text still
shows the former sequential label because that is what the application rendered
when the evidence was taken. Folder manifests, validation-asset filenames and
current product code use the corrected checkpoint identifiers. This preserves
the original pixels while making the lineage accurate going forward.

SG-128 Pack 1 is implemented. SG-128 Pack 2 visible Desktop screenshot capture
remains open.
