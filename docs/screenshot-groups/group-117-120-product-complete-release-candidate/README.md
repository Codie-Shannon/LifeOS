# Groups 117-120 - Product-Complete Release Candidate

- Screenshot group: SG-120
- Release: v27 Product-Complete Candidate
- Version: 27.0.0-rc.1
- Development commit: `fcbf9a9`

## Scope

SG-120 closes the approved Groups 117-120 implementation lane with an
end-to-end release-candidate review across Desktop, Full Mobile, Website,
documentation and evidence. Candidate status does not authorize public release,
store submission, production-provider activation or a release tag.

## Screenshots

1. `01-desktop-release-candidate-navigation.png` - reachable Settings route.
2. `02-desktop-release-candidate-overview.png` - SG-120 boundary and metrics.
3. `03-desktop-release-candidate-records.png` - completion records and approval gate.
4. `04-desktop-v27-release-version.png` - v27 release-candidate identity.
5. `05-mobile-product-candidate-navigation.jpg` - Full Mobile More navigation.
6. `06-mobile-product-candidate-overview.jpg` - Full Mobile candidate overview.
7. `07-mobile-product-candidate-checks.jpg` - Full Mobile readiness checks.
8. `08-website-release-candidate.png` - cross-product Website release gate.
9. `09-sg120-validation.png` - validation proof and uncreated-tag boundary.

## Verification

- SG-120 targeted tests: 4 passed
- Core tests: 422 passed, 0 failed
- Website tests: 28 passed, 0 failed
- Mobile tests: 54 passed, 0 failed
- Website Release publish: succeeded
- Desktop Release build: succeeded
- Android Release build: succeeded
- Public privacy scan: passed
- Repository whitespace check: passed

## Evidence-count note

This group retains nine genuine captures because Desktop, Full Mobile and
Website each provide distinct release-candidate evidence, while the validation
capture separately proves the test, build, privacy and owner-approval boundary.

## Pack boundary

This folder is Pack 2 evidence only. Product code and tests were committed
separately in Pack 1. No release tag is included; creating one requires an
explicit owner decision after evidence review.
