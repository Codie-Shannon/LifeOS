# Group 47 final validation

## Finalization baseline

- Expected pre-Pack-2 HEAD: `1745b9c33312a956aa68c1df41adf8b0e185cc11`
- Branch: `main`
- Required synchronization: `HEAD = origin/main`
- Required starting tree: clean
- Release: `v9.0.0-alpha.2`
- Evidence count: exactly 8 PNG screenshots

## Evidence coverage

The committed evidence proves:

- Integration Inbox embedded in the existing LifeOS shell;
- mixed normalized candidate types;
- full source provenance and freshness;
- deterministic duplicate comparison without silent merge;
- field-level conflict review;
- accepted-candidate links to authoritative LifeOS records;
- preview-first low-risk batch review;
- stale and source-removed/tombstone handling;
- clean synchronized repository state before Pack 2.

## Automated closure boundary

The Pack 2 runner must pass all of the following before commit:

- Group 47 evidence validation with images required;
- Core/Desktop regression tests;
- Desktop Release build;
- Website regression tests and Release build;
- Companion regression tests and Android Release build;
- NuGet transitive vulnerability scans;
- Gitleaks Git-history and Pack 2 payload scans;
- repository hygiene validation;
- staged diff formatting check;
- exact approved-path verification.

After commit, the runner pushes `main`, fetches `origin/main`, verifies `HEAD = origin/main`, verifies the working tree is clean and reruns the evidence contract.

## Safety result

All screenshot content is fictional and review-oriented. No live provider, OAuth consent, external provider read, background synchronization or external write action is included.

Group 47 is complete and closed only when the Pack 2 runner finishes successfully.
