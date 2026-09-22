#!/usr/bin/env bash
# Runs the Assisticant.UnitTest sources on any OS with the .NET SDK.
#
# The checked-in test projects are old-style .csproj files that only build on
# Windows. This script generates a throwaway SDK-style MSTest project outside the
# repo that globs the same test sources and references Assisticant.Netstandard,
# then runs `dotnet test`. Nothing in the repo is modified.
#
# Usage: run-tests.sh [dotnet test --filter expression]
#   run-tests.sh                                  # all tests
#   run-tests.sh "FullyQualifiedName~Subscription" # one class
#   run-tests.sh "Name=CanSubscribeToAComputed"    # one test
#
# Run it from anywhere inside the Assisticant checkout (or worktree) to test.
# Env: ASSISTICANT_TEST_DIR overrides where the generated project lives.

set -euo pipefail

repo_root="$(git rev-parse --show-toplevel 2>/dev/null)" || {
  echo "run-tests.sh: run this from inside the Assisticant repository" >&2; exit 2; }
if [[ ! -d "$repo_root/Assisticant.UnitTest" ]]; then
  echo "run-tests.sh: $repo_root has no Assisticant.UnitTest folder" >&2; exit 2
fi
# One build directory per checkout, so parallel worktrees don't collide.
repo_key="$(printf '%s' "$repo_root" | shasum | cut -c1-10)"
runner_dir="${ASSISTICANT_TEST_DIR:-${TMPDIR:-/tmp}/assisticant-test-runner-$repo_key}"
mkdir -p "$runner_dir"

# NotifyDataErrorInfoTests needs the WPF proxy (Assisticant.XAML), which only
# builds on Windows. AssemblyInfo is excluded because the SDK generates one.
cat > "$runner_dir/Assisticant.TestRunner.csproj" <<EOF
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <Nullable>disable</Nullable>
    <ImplicitUsings>disable</ImplicitUsings>
    <NoWarn>\$(NoWarn);CS0168;CS0219;CS0414;CS0618;CS0649;NU1701;NETSDK1215;MSTEST0001</NoWarn>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <!-- MemoryLeakTest expects a nulled local to be collectable. Tier-0 JIT code
         keeps locals alive to the end of the method, so run fully optimized. -->
    <TieredCompilation>false</TieredCompilation>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="$repo_root/Assisticant.UnitTest/**/*.cs"
             Exclude="$repo_root/Assisticant.UnitTest/obj/**;$repo_root/Assisticant.UnitTest/bin/**;$repo_root/Assisticant.UnitTest/Properties/**;$repo_root/Assisticant.UnitTest/NotifyDataErrorInfoTests.cs" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="$repo_root/Assisticant.Netstandard/Assisticant.Netstandard.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="MSTest.TestAdapter" Version="3.6.3" />
    <PackageReference Include="MSTest.TestFramework" Version="3.6.3" />
    <PackageReference Include="FluentAssertions" Version="6.12.1" />
  </ItemGroup>
</Project>
EOF

# -v q silences the build; the minimal console logger still prints each failing
# test with its message and stack, which is the evidence for intentional failures.
args=(test "$runner_dir/Assisticant.TestRunner.csproj" -c Release --nologo -v q
      --logger "console;verbosity=minimal")
if [[ $# -gt 0 && -n "$1" ]]; then
  args+=(--filter "$1")
fi

exec dotnet "${args[@]}"
