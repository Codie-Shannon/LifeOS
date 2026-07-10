# v5 Connector Readiness Matrix

| Connector | Preview contract | Live connection | First safe capability |
|---|---:|---:|---|
| Gmail / Outlook | Ready | No | User-guided thread previews |
| Google / Outlook Calendar | Ready | No | Read-only event previews |
| Xero / accounting | Ready | No | Invoice/bill previews |
| SharePoint / Drive | Ready | No | File metadata and source links |
| Receipt OCR | Ready | No | Candidate fields with source image |
| Banking | Deferred | No | No v5.0 start until security/provider review |

## Recommended v5.0 starting point

One read-only connector, one account, narrow scopes, manual refresh, preview-only intake, explicit review, duplicate handling, and no automatic target mutation.
