# Group 48 final validation

## Finalization baseline

- Expected pre-Pack-2 HEAD: `84604cf383a153f7b87c5406475c67e10c43cf93`
- Expected baseline subject: `Group 48 hotfix: prevent Microsoft provider setup crash`
- Branch: `main`
- Required synchronization: `HEAD = origin/main`
- Required starting tree: clean
- Release: `v9.0.0-alpha.3`
- Evidence count: exactly 8 PNG screenshots

## Evidence coverage

The committed evidence proves:

- two separately classified connected Microsoft identities;
- granted read-only Outlook Mail and Calendar capability while later capabilities remain Not Requested;
- bounded source-backed Outlook Message candidates;
- message provenance and attachment metadata without attachment-body download;
- bounded Microsoft Calendar candidate import;
- separate reviewable follow-up suggestions rather than silent authoritative writes;
- partial-permission recovery from an invalid calendar selection;
- restoration to a clean sync with `50 mail`, `1 calendar`, `44 suggestions`, Connected and no current provider error;
- redaction of full email addresses and opaque provider identifiers/references;
- clean synchronized repository state before Pack 2.

## Automated closure boundary

The Pack 2 runner must pass all of the following before commit:

- Group 48 evidence validation with images required;
- Core/Desktop regression tests;
- Desktop Release build;
- Website regression tests and Release build;
- Companion regression tests and Android Release build;
- NuGet transitive vulnerability scans;
- Gitleaks Git-history and Pack 2 payload scans;
- repository hygiene validation;
- staged diff formatting check;
- exact approved-path and SHA-256 verification.

After commit, the runner pushes `main`, fetches `origin/main`, verifies `HEAD = origin/main`, verifies the working tree is clean and reruns the evidence contract.

## Safety result

The committed screenshots contain no full Microsoft account address, opaque provider message/thread identifier, raw provider reference, token, authorization code, client secret, message body, attachment body or local provider configuration. The integration remains foreground-only and read-only. Background synchronization and external writes remain disabled.

Group 48 is complete and closed only when the Pack 2 runner finishes successfully.
