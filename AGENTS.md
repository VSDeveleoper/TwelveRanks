# Twelve Ranks Repository Instructions

## Application launch policy

- Never launch or activate the Twelve Ranks application except through the
  narrowly scoped WACK release-certification exception below.
- Do not use `dotnet run`, `winapp run`, Visual Studio startup commands, direct executable launches, or any equivalent command that starts the application.
- Builds and automated tests are allowed when they are relevant, but do not claim runtime behavior was verified from those checks.
- Runtime verification is user-operated. When runtime verification is needed, provide concise verification steps and wait for the user's screenshots, logs, or observations.
- Do not stop, restart, or otherwise control an application instance launched by the user unless the user explicitly requests it.

### WACK release-certification exception

- When the user explicitly authorizes a release-certification run, the agent may
  install the release package and allow the official Windows App Certification
  Kit (WACK) to launch or activate Twelve Ranks as required by its automated
  certification tests.
- This exception applies only to WACK-driven installation and activation of the
  explicitly authorized release package. It does not authorize manual launches,
  interactive runtime testing, Visual Studio startup, or any unrelated
  application activation.
- Record the exact package tested, WACK command, result, and report location.
  Do not infer broader runtime correctness from a WACK pass.
