# Group 49 — Microsoft Files evidence

Exactly eight approved screenshots close Group 49.

1. `01_Microsoft_Files_Capability.png` — the existing Microsoft provider exposes OneDrive and SharePoint as read-only capabilities, with no second registration and no external writes.
2. `02_OneDrive_Bounded_Selection.png` — an explicitly selected drive and folder use a bounded metadata-only synchronization boundary.
3. `03_OneDrive_Integration_Candidates.png` — a OneDrive-backed file enters the normalized Integration Inbox as a reviewable candidate.
4. `04_File_Provenance_And_Freshness.png` — normalized file metadata, owner, bounded source, change reference and no automatic body download.
5. `05_SharePoint_Site_Library_Selection.png` — explicit SharePoint site and document-library selection.
6. `06_SharePoint_Candidate_Project_Link.png` — a selected-library document is accepted and linked without creating a duplicate authoritative editor.
7. `07_Source_Removed_Permission_Lost.png` — source-removed and access-lost states remain visible and fail closed.
8. `08_Group49_Validation_Clean_Sync.png` — final branch, synchronization, clean-tree, Core-test and Desktop Release validation.

Evidence contains no OAuth token, client secret, authorization code, private tenant address or unredacted raw provider identifier.
