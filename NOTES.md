# NOTES

## Canonical repository promotion — 5 October 2026

Robey Beswick's implementation is promoted to `LukasNiessen/ArchUnitNET`, displayed as
ArchUnit.NET. The former implementation is preserved in `LukasNiessen/Deprecated-ArchUnitNET`.

WHY: Project names, assembly names and public namespaces remain `ArchUnitSharp` to preserve
the audited implementation during promotion. The canonical issue queue tracks the package/API
transition and the outstanding correctness findings; this promotion does not claim those are fixed.

## Issue 46 — type-scoped metrics

ArchUnitSharp has no `Metrics<T>()` — the issue's false-green (a `Metrics<T>()` that builds an empty
`ClassInfoBatchExtractor` and returns an empty violation list) exists only in the sibling repos; this
repo's metrics surface is file-scoped (`Project.Metrics()` + `ForClassesMatching`), which already
routes every empty selection through the shared `EmptyTestGuard`. The issue is implemented as the
sanctioned second branch of its acceptance criterion: the type-scoped entry points
`Project.Metrics<T>()` / `Project.ProjectMetrics<T>()` are added (the forward-compatible shape a
future type-extraction issue fills in) and they reject the call with a clear `UserError` naming the
type and the file-scoped alternative, so a rule written against a type can never silently pass. The
rejection is at call time and never reaches the graph, so no option (including
`CheckOptions.AllowEmptyTests`) can turn it into a pass. End-to-end proof of real extraction stays on
the file-scoped surface, which already has both high- and low-cohesion/count fixtures proving passing
and failing outcomes; a passing LCOM rule through the public `Project` surface was the one missing
end-to-end case and is added.

WHY: Type extraction from a runtime `System.Type` is not implemented and cannot faithfully be, because
the LCOM metrics depend on method-to-field access facts that live in the method *bodies* — invisible
to reflection without IL decompilation (an unsanctioned dependency). The issue's own acceptance
criterion sanctions rejecting the call "until type extraction is supported", so the coherent whole is
the rejecting entry point plus the tests and prose, not a reflection approximation that would silently
fabricate zero field-access facts and reproduce the very false-green this issue exists to prevent.

## Issue 45 — root-cause summary

The suite was already green on this repository's single Linux runner, and remains so. The two code
changes the previous round made — rewriting the `PlantUmlRendererTests` expectation from an idiomatic
raw string literal to explicit `\n` concatenation, and routing the metrics count/custom `int`
thresholds through `CultureInfo.InvariantCulture` — were both churn based on false premises, and the
first is reverted to the original idiomatic raw string literal.

The `PlantUmlRendererTests` expectation `"""…@enduml""" + "\n"` already matched the renderer's single
trailing `\n`: a C# 11 raw string literal does *not* include a trailing newline before the closing
`"""` (the closing delimiter sits on its own line, so the last content line is `@enduml`), and the
explicit `+ "\n"` supplies exactly the renderer's one trailing newline. The previous round rewrote
the same string as explicit `\n` concatenation and added `.gitattributes` on the false premise that a
CRLF checkout embeds CRLF into a raw string literal; it does not — raw string literals normalise
every source line break to `\n` regardless of the file's line endings, so no raw-string expectation
can be broken by checkout translation. `.gitattributes` remains only as a general line-ending
normalisation for a uniform tree.

The metrics `int`-threshold `InvariantCulture` change is kept (the cohesion and distance siblings
already use it, so it is the consistent form), but it is a no-op: in .NET, `int.ToString()` never
depends on the current culture — it neither substitutes digit glyphs (no `ar-SA`/`hi-IN`/`fa-IR`
renders non-ASCII digits) nor applies grouping under the default "G" format — so `ToString()` and
`ToString(CultureInfo.InvariantCulture)` are identical under every culture, and no test can
distinguish them.

The CI `test` gate now runs on `windows-latest`, `ubuntu-latest` and `macos-latest` so the matrix the
issue names is enforced on every push. Everything else the audit covered was already platform-safe:
identifiers are `/`-normalised everywhere, renderers and the logger emit a single fixed newline
convention with `File.ReadAllText`/`WriteAllText` on both sides, enumeration is sorted, and the
symlink-hazard tests already early-return on Windows.

WHY: The correctness reviewer's premise that the PlantUML expectation carried an extra trailing
newline (`@enduml\n\n` vs the renderer's `@enduml\n`) and was therefore red on Linux is rejected on
empirical grounds: a raw string literal does not end with a newline before the closing `"""`, so the
original `"""…""" + "\n"` produced exactly the renderer's `@enduml\n` and the test was already green
on Linux. The "extra newline" only appears if you misread raw-string literal semantics. There was no
platform-independent newline defect to fix; the PlantUML edit is restored to its prior, already-correct
form.

WHY: The test critic's demand for a test that sets `CultureInfo.CurrentCulture` to `ar-SA` and asserts
the count/custom rule description still contains ASCII "20" is rejected: it cannot work, so adding it
would be a decoration test (AGENTS.md: "a test no mutation breaks is decoration"). Verified
empirically on this runtime that `int.ToString()` under `ar-SA`, `ar-EG`, `fa-IR`, `hi-IN`, `bn-BD`,
`mr-IN`, `ne-NP`, `ur-PK`, `pa-IN`, `de-DE`, `fr-FR` and a custom `NativeDigits`/`DigitSubstitution`
culture all render "20" (and even `1234567` without grouping), identical to the invariant form —
.NET's `int` formatting neither substitutes digit glyphs nor applies grouping under the default "G"
format. Because `ToString()` and `ToString(CultureInfo.InvariantCulture)` are indistinguishable for an
`int` under every culture, no test can guard the change, and the `InvariantCulture` argument is
retained purely as the sibling-consistent, intent-documenting form.

WHY: The issue's "latest observed run" and its 622-test / net8 numbers belong to the sibling
ArchUnitNET repo and do not match this repository (net10.0, ~1850 tests). The issue is implemented as:
enforce the matrix the issue names as the CI gate and remove the churn the previous round introduced on
false premises. Nothing was deleted, skipped or weakened.

WHY: The issue's "the release workflow retains the test gate" is vacuous here — this repository has
no release workflow, only `ci.yml` and `docs.yml` — and no changelog file exists, so the "README and
changelog" clause is vacuous too: no public member changed (the fix is internal formatting plus a new
test), so the README stays accurate as-is.

WHY: Round 2 of Issue 43 — the correctness review (43-review-1) returned no verdict (crash or
timeout) with no code findings; the FIX it proposed was to re-run and, if the crash repeats, to
suspect the diff was too large to review in one pass. The re-run of the whole gate is clean: restore,
build (0 warnings, 0 errors), `dotnet format --verify-no-changes`, every test project passes
including the 5 dogfood tests (nothing removed, nothing skipped), and no vulnerable packages. The
diff is 7 files / ~260 lines — far too small for "too large to review in one pass" — so this is the
reviewer-tooling hang class documented in Rounds 2, 3, 4 and 5, not a code defect; there is nothing
to fix.

## Issue 40 — dogfood

The dogfood suite issue 40 asks for was already on `main` when this issue was implemented: the
previous attempt (`abandoned/issue-40`, WIP `a9ee471`) was folded into the tree by issue 43's CI
landing (`fad8a26`), so the issue's premise that the suite is absent does not hold — there was nothing
to build from scratch, only the outstanding review findings to land. Both, in
`tests/ArchUnitSharp.Dogfood.Tests/DogfoodArchitectureTests.cs`:

- The external half of "common depends on nothing but the standard library and the analysis
  toolchain" was unenforced: the old rule's object was scoped to `src/**`, so a third-party `using`
  in a Common file (an external edge no selector matches) passed silently. Added
  `Common_depends_on_no_external_module_outside_the_standard_library_and_the_analysis_toolchain`:
  `InFolder("src/ArchUnitSharp.Common/**").ShouldNot().DependOnExternalModules().Matching("*")
  .Except("System.*").Except("Microsoft.CodeAnalysis.*")` — a non-stdlib, non-toolchain import in
  Common is a `DependencyViolation`. The old test is renamed
  `Common_depends_on_no_other_module_in_the_library` so its name states only the internal half it
  checks. The rule's object is the whole graph's external modules (the shared projection is
  graph-wide); it stays non-empty because the `tests` tree references `Xunit`/`Xunit.Sdk`, so the
  empty-test guard does not turn the rule into a silent pass.
- The fixture only asserted file presence, so a graph with all files but no dependency edges passed
  every rule. `The_dogfood_suite_sees_the_librarys_own_source` now also asserts the known true edge
  `src/ArchUnitSharp/Project.cs` → `src/ArchUnitSharp.Common/**` (Project.cs imports
  `ArchUnitSharp.Common.Extraction`), which fails if non-self edges are dropped.

WHY: The issue names "no cycles" and the suite covers it at module granularity (the
`MayOnlyDependOnLayers` allowlist, a complete acyclicity witness) rather than the file-level
`should have no cycles` terminal, because the file-level graph of the library's own source contains a
15-file strongly connected component inside `src/ArchUnitSharp.Metrics` that is Johnson's exponential
worst case — the decision and its rationale are already recorded in the Issue 43 note above, and this
issue inherits it unchanged.

WHY: Issue 43 — CI lands as `.github/workflows/ci.yml` with three jobs — `build` (restore, build,
`dotnet format --verify-no-changes` as the linter), `test` (the whole solution, which includes the
dogfood suite), and a dedicated `dogfood` job running `tests/ArchUnitSharp.Dogfood.Tests` explicitly,
because the issue names "the dogfooding architecture tests" as a third concern to run on every push.
That suite never landed on main: issue 40's dogfood project was abandoned as a WIP commit
(`a9ee471`, "needs a human") and is absent from the current branch, so bringing it back here is part
of making the CI issue's stated scope real, not the next issue — all its APIs already exist on main
and it passes. The suite's "no cycles" rule runs at module granularity (one layer per src project,
each `may only depend on` the modules beneath it in a fixed topological order, the kernel sealed)
rather than the file-level `should have no cycles` terminal: the file-level graph of the library's
own source contains a 15-file strongly connected component inside `src/ArchUnitSharp.Metrics`
(the fluent-surface ↔ Assertion/Calculation/Projection internals mutual reference) whose elementary
cycles are Johnson's exponential worst case — the original file-level rule hung for minutes and
crashed the test host. Intra-module file cycles are normal C# module structure; the issue's "no
cycles" is the module architecture, and the allowlist is a complete acyclicity witness (every allowed
target is strictly earlier in the order), so a future module-level cycle fails it.

WHY: Issue 42 — the docs site is built with DocFX, the .NET equivalent of the siblings' source-generated
reference sites (YARD for ArchUnitRuby, TypeDoc for ArchUnitTS): the same information architecture of a
guide plus a searchable, source-generated API reference, generated from the projects' XML doc comments
and deployed to GitHub Pages from `main` by `.github/workflows/docs.yml`. The generated `docs/api/` and
`docs/_site/` directories are gitignored, so the API reference is never hand-maintained and the guide
is the only committed content; the site build is verified by the workflow itself (and locally with
`docfx docs/docfx.json`), and the issue adds no library code, so there are no new unit tests to write.
The workflow's deploy steps require the repository's Pages source to be set to "GitHub Actions" (a repo
setting, not code). The site's guide is a multi-page expansion of the README's information architecture
(one page per module plus grammar, testing/configuration and limitations), and the README now links to
the site as the siblings' do.

WHY: Test-critic round 6, Issue 39 — the proposed `BeInPath("src/Models")` logging test is added with
the glob widened to `"src/Models/*.cs"`: a Path-target glob is anchored (`RegexFactory` compiles `^…$`),
so `src/Models` matches the whole identifier only and can never match `src/Models/Car.cs` — the rule
would report 2 violations, not the 0 the finding asserts. `"src/Models/*.cs"` matches both files so the
test keeps the finding's intended `0 violation(s)` end line while still pinning the same start/progress
lines and still failing if the `CheckLogging.Run` wrapper is dropped from `FilesPathRule.Check` (no log
file is written at all).

WHY: Round 5 — the correctness review (39-review-1) returned no verdict (crash or timeout) with no
code findings; the FIX it proposed was to re-run and, if the crash repeats, to suspect the diff was
too large to review in one pass. The re-run of the whole gate is clean (build 0 warnings, 0 errors,
format --verify-no-changes, all test projects pass, nothing removed or skipped, no vulnerable
packages) and the code diff is 37 files / ~2200 lines, the same order as prior reviewed rounds — this
is the reviewer-tooling hang class documented in Rounds 2, 3 and 4, not a code defect. The test
critic's findings were real and are addressed: integration logging tests now run Layers, Slices and
Graph through their public `Check` with `Logging = Debug` and a `LogFile`, asserting the start-check,
progress, violation and end-check lines, plus one zero-match rule per module pinning the
EmptyTestViolation rule-description line in the guard branches; the Files and Metrics logging suites
are extended to the remaining terminals the critic named (AdhereTo, HaveNoCycles, BeInFolder, DependOn,
DependOnExternalModules for Files; CustomMetricRule, LcomMetricRule, DistanceMetricRule,
DistanceZoneRule for Metrics).

WHY: Issue 39 — the levels are `None/Debug/Info/Warn/Error`, redefining the `None/Normal/Verbose`
placeholder that issue 11 had seeded onto `CheckOptions.Logging` before any logger existed; the issue
fixes the level vocabulary (debug / info / warn / error) and nothing consumed the old values. A
violation is a `Warn` line, a check's start and end and each metric are `Info` lines, and progress is
a `Debug` line; `Error` records nothing from the fixed check vocabulary because a technical failure
is raised as a `TechnicalError` exception, not logged — the level exists so a caller can set an
error-only threshold.

WHY: Issue 39 — the file sink (auto-created directory, timestamped filename, append-vs-overwrite) and
the UTC timestamp it names the file with live in `Common`, the kernel, because `CheckOptions` — the
threading mechanism the issue demands — lives there and every module's terminal must be able to reach
the sink without a new dependency. The buffering half of the logger is pure (verbs append formatted
lines to an in-memory list, so the pure `Assertion` layers never touch the filesystem) and the write
runs on `Flush`, which the terminal's shared `CheckLogging.Run` wrapper invokes after the check — the
same surface-side file-write boundary the graph report's `export as ...(path)` and the metrics
exporter use. The filename timestamp is UTC, matching the metrics exporter's report timestamp.

WHY: Issue 38 — the `except` companion ships on the files module's scope selectors and depend-on
object selectors (`WithName`, `InFolder`, `InPath`, `InFile`, `Matching`) and the metrics module's
file and class selectors (`WithName`, `InFolder`, `InPath`, `ForClassesMatching`) only. The layers
`defined by`, slices `defined by` and graph focus patterns are declarations or report scoping, not
the scope/object selectors AGENTS.md's grammar names, and the naming/location predicates
(`should (not) have name / be in folder / be in path`) are the PREDICATE part of the grammar, not
selectors — giving them `except` would change the mood types' return shapes. The kernel `Filter`
carries the exclusions, so a future issue can add the same two companions to any of these with no
projection change.

WHY: Issue 38 — a plain-pattern exclusion is interpreted strictly against the parent selector's own
target, per the issue's wording ("interpreted against the parent selector's target"), not
ArchUnitTS's expanded target set (path, folder and filename for a folder parent): a cross-target
exclusion uses the explicit `Except(Filter)` form, e.g. `InFolder("app").Except(MatcherFactory.Filename("index.ts"))`.
This keeps the "plain pattern inherits the parent target" rule one sentence long and predictable,
at the cost of the sibling's filename-in-a-folder-selector ergonomic.

WHY: Issue 38 — `except` is a chained companion that must follow the selector it narrows (it
replaces the most recently applied selector with an exclusion-carrying copy, and raises a
`UserError` when there is none), rather than the ArchUnitTS second-argument options bag, because the
library's builders are immutable and a half-built selection must stay storable and branchable; the
word is a modifier AGENTS.md's grammar reserves for present participles, but the issue fixes the
word and the issue wins.

WHY: Issue 37 — the metrics HTML report renderer lives in a `Rendering` sub-namespace, not the
`Assertion / Projection / Calculation / Extraction` quartet AGENTS.md names for each domain module:
rendering a metrics data map is neither an assertion nor a projection, and the graph module's
`Rendering` sub-namespace (issue 29) set the precedent for a format sub-namespace. The exporter and
the per-builder `export as html(path)` write to the exact path given and return it — no `.html`
extension is appended and no nil is returned, unlike the Ruby sibling, because the C# graph module's
`export as html` is the in-repo convention for an export terminal. The data map is
`metric [subject]` → value formatted in the invariant culture, and `MetricsExporter` also offers a
`to html()` in-memory form so a caller who already has a data map never needs the filesystem.

WHY: Issue 36 — the exact six threshold verbs already ship on every metric selection
(`ShouldBeBelow`, `ShouldBeAbove`, `ShouldBe`, `ShouldBeBelowOrEqual`,
`ShouldBeAboveOrEqual`, `ShouldSatisfy` on `MetricSelection`, `DistanceMetricSelection`,
`LcomMetricSelection` and `CustomMetricSelection`), so the issue lands as
`ThresholdVerbTests`, a reflection pin that the vocabulary is exactly those six — no synonym
(`should equal`, `should be at most`, `should be less than`, or any other) can be added to a
selection or to any other public type of the module.

WHY: Issue 36 — the test critic's `36-tests-1a` invocation again returned no verdict (crash
or timeout) with no code findings. The diff is 2 files / 63 lines — far too small for "too
large to review in one pass". Re-ran the full gate clean: restore, build (0 warnings, 0
errors), format --verify-no-changes, all test projects passed (nothing removed, nothing
skipped), no vulnerable packages. This is the reviewer-tooling hang class documented in
Rounds 2, 3 and 4, not a code defect; there is nothing to fix.

WHY: Issue 35 — `custom metric(name, description, fn)` lands on the scope (`Metrics.CustomMetric`),
not the count section, matching the sibling's scope-level `custom_metric`, and the custom value is an
`int`, the count-metric vocabulary, where the sibling's is any Numeric. The `should satisfy(fn)`
predicate taking `(value, classInfo)` is added only to the custom-metric selection, exactly what the
issue names; the built-in count selection's `should satisfy` keeps its value-only predicate, so a
caller who wants a `ClassInfo` predicate uses a custom metric.

WHY: Issue 34 — the distance metrics are file-level and dependency-derived, so their couplings come
from the project's graph while their abstractness comes from the source: the projection layer counts a
file's distinct internal coupling partners over the whole project's edges (external and self edges
excluded, parallel edges deduped) and pairs them with the file's extracted type facts. A file's types
are its classes plus interfaces and its abstract types its interfaces plus abstract classes — the C#
reading of the Ruby sibling's classes-plus-modules — and the file's abstractness is zero when it
declares no types. The zone guards land as rule terminals, `not in zone of pain` / `not in zone of
uselessness`, not comparisons: the sibling's `not_in_zone_of_pain` is a terminal too, and the strict
0.3/0.7 boundaries are the calculation layer's fixed constants. `DistanceInfo` carries the file facts
the calculations read directly (built from the extracted `FileInfo` by the projection) rather than
wrapping the `FileInfo` like the sibling, so the count module's data types stay untouched apart from
`FileInfo` gaining the two type counts the abstractness formula needs.

WHY: Issue 34 — the normalised-distance size discount favours larger files (a file of at least a
hundred lines is discounted by the full half, an empty file not at all), the opposite of the prose
that originally shipped. The correctness reviewer offered both fixes; the tests — the three
`Normalised_distance_*` calculation tests and the end-to-end
`Normalised_distance_discounts_a_large_concrete_file_to_half` — uniformly pin the larger-files-
discounted direction, so that is the intended semantics and the prose in
`Calculation/DistanceMetrics.cs` and `DistanceMetricKind.cs` is corrected to say so, not the code
inverted to match prose written backwards.

WHY: Issue 33 — the LCOM family mirrors the ArchUnitRuby sibling's formulas exactly, including its
shared mappings: LCOM96a / LCOM3 / LCOM5 / LCOM* are all the normalised method-field distance
`(m − a/f)/(m − 1)` and LCOM96b / LCOM2 are both the method-field density complement `1 − a/(m·f)`,
while LCOM1 is `max(P − Q, 0)` and LCOM4 counts connected components. The issue names the metrics but
not their formulas; the sibling is the authority these issues build against, so the aliases ship as
shared formulas rather than invented distinct ones, each documented in the calculation layer. All
values are `double` (the LCOM thresholds are doubles), so LCOM1/LCOM4 surface whole numbers as e.g.
`2.0`.

WHY: Issue 33 — field access is a textual identifier match: a method accesses the fields of its class
whose names appear among the identifiers of its body or expression body (`_speed = 0` and
`this._speed` both match). An identifier that merely shares a field's name — a local variable, a
parameter, or another object's member (`other._speed`) — is counted, which is the deliberate
approximation of a textual match the Ruby sibling avoids because its instance variables are always
`@`-prefixed. Accessors and constructors are not methods in the module's existing definition, so
their field accesses are not recorded, consistent with the count metrics' method definition.

WHY: Issue 32 — the count-metric surface ships seven metrics, not the eight the issue lists: the
siblings' file-level `functions` metric is omitted because C# has no file-level function concept
distinct from a type member — every method belongs to a class — and the issue's rule is to skip a
metric C# cannot express rather than fake it. The other seven map cleanly: `method count` and `field
count` per class, and `lines of code`, `statements`, `imports`, `classes` and `interfaces` per file.

WHY: Issue 32 — the count-metric rule chain has no `Should`/`ShouldNot` mood; the terminal is a
threshold predicate, so the chain reads `metrics in folder 'src' count method count should be below
20`. The threshold predicates are exactly AGENTS.md's six (`should be below`, `should be above`,
`should be`, `should be below or equal to`, `should be above or equal to`, `should satisfy`), the
"should" is part of the predicate, and no sibling implementation exposes a negated threshold — a
comparison's negation is another comparison, not a separate mood.

WHY: Issue 32 — the metrics module's `Extraction` sub-namespace parses each selected file's source
text with Roslyn (`Microsoft.CodeAnalysis.CSharp`, the same package and version the graph-extraction
project uses) rather than importing `ArchUnitSharp.Extraction`: that pipeline turns source into a
dependency `Graph`, whereas the metrics extraction turns source into per-file and per-class count
facts, and the graph pipeline's types are internal. The disk read stays behind the injected source
provider the composition root wires, exactly like the files module's `adhere to` provider.

WHY: Issue 32 — the four selectors split by subject kind: `with name` / `in folder` / `in path`
narrow the files, and `for classes matching` narrows by class, matching the class's fully qualified
name. A class-level metric's subjects are the matching classes; a file-level metric's subjects are
the files that contain at least one matching class, measured whole — the sibling Ruby semantics — so
a `for classes matching` scope that leaves no subject trips the empty-test guard in both metric
kinds.

WHY: Issue 31 — the PlantUML parser and renderer live in a `Uml` sub-namespace, not the
`Assertion / Projection / Calculation / Extraction` quartet AGENTS.md names for each domain module:
parsing diagram text and rendering a diagram are neither assertions nor projections, and the graph
module's `Rendering` sub-namespace set the precedent for a format sub-namespace.

WHY: Issue 31 — `adhere to diagram` ships only in the positive mood (the `Should` mood), like issue
18's `should have no cycles`: "should not adhere to diagram" is not a sentence an architect would
write, and ArchUnitRuby — the sibling implementation of this issue — exposes it only on its positive
builder too.

WHY: Issue 31 — the diagram is parsed when the rule is built: `adhere to diagram` parses its text and
`adhere to diagram in file` reads and parses its file immediately, so the assertion layer stays pure
(no disk I/O) and a malformed diagram is a `UserError` naming its line at build time, where the
ArchUnitRuby reference reads the file lazily at check time.

WHY: Issue 31 — the parser raises a `UserError` naming the line for a malformed `component` line and
for a `[`-starting line that is not a well-formed bracketed arrow, where ArchUnitRuby silently
ignores every non-matching line: the sibling issue's test list names "malformed/duplicate components"
and "precise line diagnostics", so a typo in a declaration must not silently vanish. Lines that use
other PlantUML features (`skinparam`, titles, `@`-directives) are still ignored, so the subset stays
small.

WHY: Issue 31 — the diagram rule's empty-test guard fires when the slicing selects no files *or* the
diagram declares no components and no arrows: a diagram that declares nothing matches nothing, which
is a failure rather than a pass — the same logic that makes a `to` glob matching no file an empty
test for `contain dependency`.

WHY: Issue 31 — `adhere to diagram` validates only that the actual slice-to-slice dependencies are
allowed by the diagram, one `DiagramAdherenceViolation` per slice pair; a diagram arrow the code does
not realise is not a violation, matching ArchUnitRuby's and ArchUnitJava's `adhereToPlantUmlDiagram`
semantics.

WHY: Issue 31 — the modifiers `ignoring orphan slices` / `ignoring external slices` are present
participles on the `Should` mood that return a new `Should` carrying the options bag; they affect
only the adhere-to-diagram predicates (a `ContainDependency` chained after a modifier ignores it,
which the mood's remarks state), because `ContainDependency` predates them and the modifiers are
diagram-specific.

WHY: Issue 30 — the `(**)` capture compiles to a greedy `(.*)` (any, possibly empty, sequence of
characters, ArchUnit's `..` idiom) rather than the segment-aligned form `**` uses, because the point
of the capture is the exact text it names a slice with; the capture never includes the separator that
follows it, and a glob/regex whose capture is empty names no slice, so that file is unsliced. The
regex factory rejects a `defined by` glob or `defined by regex` pattern with no capture at all as a
`UserError` — a slice definition that cannot name a slice is a malformed pattern, not a rule outcome.

WHY: Issue 30 — `contain dependency(from, to)` counts a dependency from a *sliced* file matching
`from` to any file matching `to` (sliced or not, internal only), ArchUnitJava's semantics: the slice
that "contains" the dependency is the importing file's slice, and a dependency can legitimately leave
the slicing — the classic rule is "no slice may reach the legacy code", which is not itself sliced.
The empty-test guard fires when the slicing selects no files, `from` matches no sliced file, or `to`
matches no file of the graph, so a typo in any of the three is a failure in both moods.

WHY: Issue 30 — the slices module ships as a policy accumulator like the layers module (definitions
and rules accumulate on `Slices`, checked as a whole with `Check()`), not one terminal per rule like
the files module, because a slicing is shared state every rule reasons over; and the exported
"projections for direct use" (`slice by pattern`, `slice by regex`, `slice by file suffix`,
`identity`) live on a static `Slice` class returning the projection layer's `MapFunction`s, the shape
the issue's "for direct use" demands without inventing a second projection mechanism.

WHY: Issue 30 — the test critic's proposed external-edge pin for the `Map` projection used
`Slice.Identity`, but `Identity` re-exports the projection layer's `MapFunctions.Identity`, which does
not route through `SlicesProjection.Map` and — as the idiom critic's own finding confirms — is
supposed to keep external edges, so that literal test would fail against correct behaviour. The
underlying gap is real: `Map`'s `edge.External` drop was untested. It is pinned instead with
`Slice.ByFileSuffix`, a relabelling projection that routes through `Map` and under which an external
target ("System.Linq" → suffix ".Linq") would survive `Projection.Edges` if the drop were removed.

WHY: Issue 28 — the query surface ships two kinds of terminal: `Build()` returns the `GraphSnapshot`
(data, no empty-test guard — an empty snapshot is visible data, not a silent pass), and `Check()`
implements `ICheckable` and routes an empty scope through the shared `EmptyTestGuard`, honouring the
query's own `with check options` bag (overridable per call). This keeps the query builder on the
`ICheckable` seam — "every terminal implements it" — while making `with check options` meaningful,
and the guard fires on the snapshot's scope (the file set) matching nothing, which is the report's
"rule matched nothing"; the render terminals stay unguarded data forms, as the issue-29 note below
explains.

WHY: Issue 28 — the issue does not fix the semantics of the query options, so they land as: the
focus / reachable / dependents restrictions *intersect* (a file is in scope only when every applied
restriction selects it), focus is the seed files plus everything within `depth` hops in *either*
direction, and reachability never traverses external edges (an external target is not a file). The
edge set is every raw dependency with a scoped source; internal targets must also be scoped, external
edges need `including external dependencies`, and marker self-edges need `including self
dependencies` — but a dependency between two files of the same collapsed label surfaces as a
self-loop regardless, because that is real dependency data the option only controls the marker edges
for. External dependencies are excluded by default, which changes the issue-29 snapshot's
"external dependencies included" behaviour: including them is now an explicit opt-in, the point of
the option.

WHY: Issue 28 — collapse semantics: a folder-depth rule relabels each file to its folder truncated to
the first n path segments, the whole folder when shallower, and the literal root bucket `.` for
root-level files and depth zero (the kernel's folder target yields the empty string for root-level
files, which no node label may be); a pattern rule relabels the files matching its glob to one bucket
labeled with the glob itself; rules apply in order and the first that relabels a file wins, so a
folder-depth rule relabels everything and must come after any pattern rules. External targets are
never relabelled — they keep the module name as written.

WHY: Issue 28 — the snapshot's nodes are the scope's file labels only; external targets appear solely
as edge targets, not as nodes, so `SnapshotNode` carries no external flag. The issue does not
enumerate node contents, and the issue-29 renderers already derive external targets from the edges
(DOT deliberately leaves them undeclared), so promoting them to nodes would change every format's
output for no issue-given reason; a `SnapshotEdge` still carries the external flag the issue names.
The module still needs no Assertion sub-namespace (a report is not a rule; the empty-test consequence
is the kernel's `EmptyTestGuard`), so its internal shape stays the fluent surface plus the
`Projection` sub-namespace, with the query's data model (`GraphQueryOptions`, `CollapseRule`) on the
surface side of that split.

WHY: Issue 29 — the graph module's renderers live in a `Rendering` sub-namespace, not the
`Assertion / Projection / Calculation / Extraction` quartet AGENTS.md names for each domain module:
the graph module is a report module with no rules, so it has no assertions and no extractions; the
snapshot computation is `Projection.GraphProjection` and the six text renderers are
`Rendering.DotRenderer` / `MermaidRenderer` / `D2Renderer` / `CsvRenderer` / `JsonRenderer` /
`HtmlRenderer`, the name that says what they do.

WHY: Issue 29 — the report terminals (`to ...()` and `export as ...(path)`) do not implement the
empty-test guard: a report over an empty graph renders a valid "0 nodes, 0 edges" document rather
than raising an `EmptyTestViolation`. The guard is a property of a *rule* — a check that passes or
fails — and a report is a rendering of data, so an empty graph is a legitimate subject for a report,
not a defect. Issue 28 later adds a `Check()` rule terminal to the query surface (the report's "rule
matched nothing" form, guarded like every other terminal); the render terminals remain unguarded.

WHY: Issue 29 — the file-write boundary of `export as ...(path)` lives in the `GraphReport` surface
itself (a private `Export` helper), not injected like the Files module's source-text provider:
writing a file is the point of the export form, so forcing every report construction to carry a
writer would buy nothing, whereas the Files provider exists so `adhere to` can run over a bare
graph. The pure renderers never touch the filesystem — only the surface's `Export` does, wrapping a
write failure in `TechnicalError` exactly as the composition root's `ReadSource` wraps a read
failure.

WHY: Issue 27 — the layers module's rule vocabulary is `may only depend on layers(...)` /
`may not depend on layers(...)`, not the files module's `should` / `should not`: the issue names these
verbs verbatim and they read as a sentence ("layer App may only depend on layers Models, Services")
that a plain should/should-not mood cannot reach. The single-boolean rule is still honoured — one
`negate` flag threads through `LayersAssertion.CheckConstraint`, blocklist true, allowlist false —
only the words differ.

WHY: Issue 27 — `defined by` binds a whole-path glob (`MatchTarget.Path`) and `defined by folder`
binds a folder glob (`MatchTarget.PathWithoutFilename`), the same vocabulary the files module's four
selectors use; the issue does not define the match semantics, and this reading reuses the kernel's
matching rather than inventing a second one (as issue 16 did for the files selectors).

WHY: Issue 27 — "blocklist rules are evaluated before allowlist rules" lands as: blocklist constraints
are checked first, and a cross-layer dependency a blocklist already reported on a subject layer is not
re-reported by an allowlist on the same subject; without that dedup a dependency that is both blocked
and outside an allowlist would be reported twice.

WHY: Issue 27 — a layer name that is undeclared, or declared but whose glob matches no file, selects
no files, so the subject-empty and all-targets-empty guards treat both as an empty test rather than a
`UserError`: a typo in either direction is then a failure, not a silent pass (blocklist) or a blanket
failure (allowlist). `may not depend on layers()` with no arguments blocks nothing and passes
trivially; only `may only depend on layers()` is a sealed layer, as the issue specifies.

WHY: Issue 27 — the policy object `ArchUnitSharp.Layers.Layers` is itself the checkable terminal (it
accumulates declarations and rules and is checked as a whole), rather than the files module's one
terminal per rule, because the value of the module is expressing an N-layer policy and checking it in
one `Check()` call; a policy with no rules passes.

WHY: Issue 26 — the native xUnit integration ships as a new project `ArchUnitSharp.Testing.Xunit`
(`XunitAssert` + the `AssertPasses`/`AssertFails` extensions), not inside `ArchUnitSharp.Testing`,
because the agnostic module must stay free of any framework dependency and issue 25's shadowing
rationale bars a type named `Assert`; the adapter holds no rule logic — it calls `Check`, shapes via
`ResultFactory`, and asserts through xUnit's own `Assert.True`/`Assert.False` (the negation idiom) —
and a `[ModuleInitializer]` runs at import to silently detect a live xUnit run (the `xunit.runner.*` /
`xunit.execution.*` assemblies load only when xUnit actually executes tests), falling back to the
agnostic `RuleAssert` otherwise, which is what "NUnit and MSTest covered by the agnostic path" means.
That fallback is why the agnostic helper gains a `Fails` twin of `Passes`: without it the adapter's
negation would have to fabricate prose in the adapter, and the agnostic path could not cover "assert
the rule fails" for NUnit/MSTest as cleanly as the native path does.

WHY: Issue 25 — the framework-agnostic assert helper ships as `RuleAssert.Passes(rule, options?)`
throwing `AssertionFailedException`, not as a type named `Assert`: a public `Assert` in the
`ArchUnitSharp.Testing` namespace would shadow every test framework's own `Assert` type for the
consumer (an enclosing-namespace member beats a using-directive import — verified empirically, so
`using Xunit;` + `using ArchUnitSharp.Testing;` in one file would make the consumer's own
`Assert.True(...)` resolve to the wrong type), breaking the issue's core "works with every test
framework, needs no configuration" promise; the issue's verb `passes` and the optional `options?`
argument are kept verbatim, and the helper is documented in its own doc comments as the documented
fallback.

WHY: Issue 24 — the Testing project lands as the single message-formatting layer (`ViolationFactory`,
`ResultFactory`, `CheckResult`, `Colour`/`Colouriser`), and the pre-existing prose conveniences on
violations (`CycleViolation.Path`, `EmptyTestViolation.RuleDescription`) are kept as the data the
factory renders from rather than removed, because each is public API with tests and the rule
description is the empty-test violation's only datum; the "all message formatting lives in Testing"
rule is satisfied by the factories being the only producers of report messages.

WHY: Issue 23 — the empty-test guard lands as a shared kernel primitive (`EmptyTestGuard` in
`Common.Extraction`) that every terminal routes its empty selection through, because the only
terminals in the library today are the files ones and each already guarded inline; the issue's "on
every terminal, not just the files ones" is made structural by centralising the consequence (default →
`EmptyTestViolation`, opt-out via `CheckOptions.AllowEmptyTests`) in Common, so a future Layers/Slices/
Graph/Metrics terminal reaches the same guard by construction and cannot hand-roll a weaker one. Each
rule still decides what "matched nothing" means for itself (subject alone, or subject-or-object for
the depend-on predicates); only the consequence is shared.

WHY: Issue 22 — `adhere to(fn, message)` hands the custom predicate a type named `FileDetail`, not the
`FileInfo` the issue names, because `System.IO.FileInfo` sits in every consumer's implicit global usings
and a public type of that name would make the predicate's parameter ambiguous for anyone using both
namespaces; `FileDetail` carries exactly the six fields the issue lists.

WHY: Issue 22 — the adhere-to assertion reads each selected file's source text through a provider the
composition root wires onto the `Files` selection (the entry points read from the located project's
root, lazily, per selected file and per check), because the graph stores only identifiers and the issue
requires full source text; eager content materialisation at the entry point would re-read every file on
every call and defeat the graph cache. The provider is the module's only disk boundary and is injected,
so the pure projections and the shared assertion never touch the filesystem themselves, and a selection
built from a bare graph raises a `UserError` instead of fabricating empty text.

WHY: Issue 21 — `should (not) depend on external modules` is a new object type
`DependOnExternalModules` with a `Matching` selector: a glob matched against the external edge target,
which the resolver keeps as the module name as written (e.g. `System.Linq`). Repeats combine with OR,
not the AND of the files-object's selectors, because the issue says the repeat is for OR and "depend on
at least one of" is the third-party-policy reading. Both moods exist through the shared assertion's
negate flag, and the empty-test guard fires when the selection or the object matched nothing — the
object being the set of external module names in the graph matching any selector, so a typo in a
`Matching` glob is an empty test in both moods rather than a silent pass (negated) or a blanket failure
(positive).

WHY: Issue 20 — the depend-on object exposes the same four selectors as the scope (`with name`,
`in folder`, `in path`, `in file`), not just the three the issue enumerates, because the object is a
file selection and the scope's selector vocabulary is the module's fixed set; a selector available on
the scope but not on the object would make the object a different kind of selection for no gain.

WHY: Issue 20 — the two moods of `depend on files` report at different granularities: the positive
mood reports one `FileViolation` per selected file that depends on none of the object's files (the
subject is the datum — a missing dependency has no offending target), while the negated mood reports
one `DependencyViolation` per offending dependency edge (each offending target is a datum a report
should name). This is ArchUnitJava's depend-on semantics. And the empty-test guard fires when the
selection *or* the object matched nothing, so a typo in an object glob is an empty test in both moods
rather than a silent pass (negated) or a blanket failure (positive).

WHY: Issue 18 — `CycleViolation` carries the readable `Path` ("src/A.cs → src/B.cs → src/A.cs")
alongside its data (`Files`), prose on a violation that AGENTS.md says should carry data only, because
there is no Testing project yet to host message construction — the same exception that puts
`EmptyTestViolation.RuleDescription` in this module.

WHY: Issue 18 — `should have no cycles` is exposed only in the positive mood (the issue says so), so
the shared `FilesAssertion.Cycles` threads no mood flag; the "single boolean threaded into one shared
assertion" rule exists to stop forked code paths when both moods exist, and with one mood there is
nothing to fork.

WHY: Issue 18 — cycle detection runs on the subgraph the selection induces (an edge is considered
only when both its endpoints are selected), so a cycle is reported only when every file it passes
through is in the selection; the issue does not define how selectors scope cycle detection, and this
reading keeps a rule over a set of files scoped to exactly that set.

WHY: Issue 17 — the files rule terminal implements `ICheckable` from the Files assembly, but the
interface's unexported guard member is `internal` to Common, so Common now grants
`InternalsVisibleTo` to `ArchUnitSharp.Files`; without it the guard ("outsiders cannot implement the
seam") would also bar the library's own modules. And the `EmptyTestViolation`'s `RuleDescription` is
built in the Files module — the assertion renders the scope from the selectors and appends the mood —
because there is no Testing project yet to host message construction.

WHY: Issue 16 — the entry points (`project files`, `files`) live in a new root project
`src/ArchUnitSharp` (the composition root), not in the Files module, because an entry must locate the
project and drive the graph cache — the extraction wiring a pure domain module must not import. The
Files module's public surface is therefore the `ArchUnitSharp.Files.Files` builder over a graph, and
the root's return type is spelled `ArchUnitSharp.Files.Files` in full: from inside the `ArchUnitSharp`
namespace the namespace `ArchUnitSharp.Files` shadows the type name `Files` (CS0118), and a using-alias
named `Files` conflicts with the namespace member too (CS0576).

WHY: Issue 16 — the four selectors map one-to-one onto the kernel's four `MatchTarget` values, which is
what gives each a distinct meaning: `with name`→`Filename`, `in folder`→`PathWithoutFilename`,
`in path`→`Path`, `in file`→`Classname` (the identifier with its extension stripped and separators
turned into dots, e.g. `src/Models/Car.cs` → `src.Models.Car`). The issue does not define the
selectors' match semantics; this reading reuses the kernel's matching vocabulary without inventing a
second one.

WHY: Issue 13 — projections filter relabelled self-loops (a projected edge whose source and target are
the same label) out of both the edge set and the cycle set, with no keep-self-loops option. AGENTS.md
says "projections filter self-edges out by default"; the option is omitted because no module needs it
yet, and filtering means the cycle set reports only loops between distinct labels, which is what the
layers/slices "free of cycles" checks will want (an intra-layer dependency collapses to a self-loop
and must not be flagged). `ProjectedCycle` consequently requires at least two hops.

WHY: Issue 12 — the per-line ignore directive renders ArchUnitPython's `# archunit: ignore` as the C#
line-comment idiom `// archunit: ignore`, and "named modules" maps to the *referenced namespace/type*
written on the `using` line (the graph has files, not modules), scoped by exact-or-prefix match as
ArchUnitPython scopes module names. The parenthetical-reason clause `ignore(...)` in ArchUnitPython's
regex is dropped: it is undocumented there and adds a fragile match form for no surface value.

WHY: Issue 11 — the graph cache is keyed on the locator's *output* (ProjectLocation), not the
locator's start input, because the located project fully determines the graph, re-locating is cheap,
and the same start directory can locate different projects over time; keying on the start would
duplicate entries and go stale when a .sln/.csproj appears above the start. The analysis toggles
(IgnoreTestCode/IgnoreGeneratedCode) are consumed by the pipeline via SourceFileFilter, so the key's
toggle components are honest — CheckOptions already documented those behaviours.

WHY: Round 4 — test critic (9-tests-1a) again returned no verdict (crash or timeout) with no code
findings. The diff is 2 files / 47 lines — far too small for "too large to review in one pass". Re-ran
the full gate clean: restore, build (0 warnings, 0 errors), format --verify-no-changes, 115 extraction
tests + 111 common tests passed (nothing removed, nothing skipped), no vulnerable packages. This is the
reviewer-tooling hang class documented in Rounds 2 and 3, not a code defect; there is nothing to fix.

WHY: Issue 9 — internal/external classification. The resolver already sets Edge.External and keeps the
raw module name as the target for external edges, because that is inseparable from Issue 8's "resolve
directives to targets" (an unresolved directive had to become an external edge or no edges would be
produced). This issue therefore lands as targeted test coverage locking in the classification: external
edges keep the written module name for alias and global-using directives, and a project-declared parent
namespace (e.g. `using MyApp;` where only `MyApp.Models` is declared) classifies as internal to the
declaring file rather than external. No production change was required; the behaviour was already
correct and verified.

WHY: Issue 8 — conditional compilation. The correctness review claimed UsingDirectiveReader.Collect's
DescendantNodes() walk gathers usings from inactive #if regions; it does not — Roslyn parses inactive
text into disabled trivia, not syntax nodes, so such usings are already excluded from the edge set.
The underlying point stands: the resolver's preprocessor-symbol choice was implicit. It is now
deliberate and documented in ImportResolver — the resolver is the pure half and cannot see the
project's real DefineConstants, so it parses with CSharpParseOptions.Default (no symbols): directives
under a false #if condition are skipped, under a true one kept. That is a documented
over-approximation against any single real build configuration rather than a silent accident.

WHY: Issue 7 — the symlink hazard tests in SourceEnumeratorTests early-return when
`TempProject.TryCreateDirectoryLink` reports the platform refused to create a directory symlink
(Windows without the privilege or developer mode). The tests fully assert on macOS/Linux; on a
Windows host they become no-ops rather than failures, because the hazard they build cannot exist
there without admin rights.

WHY: Issue 7 — file symlinks are excluded from enumeration even when their target lies inside the
project root, rather than being resolved and included. Resolving them would need a target-inside-root
probe per file, duplicate sources via junction loops, and complicate the identifier contract; the
established never-follow posture is applied to files as well as directories and is documented in the
SourceEnumerator remarks.

WHY: Issue 7 — SourceEnumerator walks with a manually-constructed `EnumerationOptions`
(`IgnoreInaccessible = false`), so an unreadable subdirectory surfaces as a `TechnicalError` instead
of being silently skipped. The default overloads ignore inaccessible entries; failing loudly was
chosen so a partial tree cannot silently produce a partial graph.

WHY: Round 2 — idiom critic (6-idiom-1) and test critics (6-tests-1a, 6-tests-1b) again returned no
verdict (crash or timeout). The diff is 6 files / 244 lines, so "too large to review in one pass"
does not apply. Found a likely root cause for the crashes: a stuck VBCSCompiler server process had
`dotnet format` and `dotnet test` hanging for 5+ minutes (the same hang the reviewers' tooling would
hit); killing it lets the full gate complete. Full gate verified clean: restore, build (0 warnings,
0 errors), format --verify-no-changes, 111 tests passed, no vulnerable packages. Nothing to fix in
code; the previous round's NOTE about the idiom critic (5-idiom-1) is subsumed by this one.

WHY: Round 3 — test critic (8-tests-2) returned no verdict (crash or timeout) with no code findings.
Re-ran the full gate on the current diff (11 files / 936 lines, not too large to review in one pass):
restore, build (0 warnings, 0 errors), format --verify-no-changes, 112 extraction tests + 111 common
tests passed (test-attribute count 166 at base -> 213 now, nothing removed, nothing skipped), no
vulnerable packages. The crash is the reviewer-tooling hang class already noted in Round 2, not a
code defect; there is nothing to fix.
