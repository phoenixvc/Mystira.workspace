// This namespace is kept alive for backward compatibility.
// All repository interfaces have been consolidated into Mystira.Application.Ports.Data.
// Consumer files that still have "using Mystira.Application.Ports.Data;" will compile
// because this namespace exists (as a valid empty namespace), and the actual types
// are resolved via the global using of Mystira.Application.Ports.Data in GlobalUsings.cs.
namespace Mystira.App.Application.Ports.Data;
