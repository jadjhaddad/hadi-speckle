# Speckle Objects & Converters Documentation Index

## Overview

This comprehensive documentation suite provides complete coverage of the Speckle Objects kit and converter system. Three main documents are included, each serving a specific purpose.

---

## Document Map

### 1. **OBJECTS.md** (34 KB, 1,200 lines)
**The Core Interoperability Kit Reference**

**Purpose:** Understand the domain model, object types, and the Objects kit architecture.

**Key Sections:**
1. Executive Summary - What is Objects?
2. Project Structure - Objects.csproj, dependencies
3. Domain Model Organization - 8 categories of objects
4. Objects Kit - Type enumeration and converter discovery
5. Converters Directory - 60+ projects overview
6. Converter Pattern - ISpeckleConverter interface
7. Bidirectional Conversion - ToNative vs ToSpeckle
8. Converter Discovery - Runtime loading mechanism
9. Adding New Object Types - Complete step-by-step guide

**Read this when you need to:**
- Understand what objects exist
- Add new domain model types
- Understand the converter loading system
- Learn object hierarchy and interfaces
- Debug object serialization issues

**Contains:**
- 200+ object type listings
- 8 domain category descriptions
- Complete ObjectsKit.cs source
- Detailed converter discovery algorithm
- 8-step guide to adding custom objects
- Code examples for new object types

---

### 2. **CONVERTERS.md** (36 KB, 1,210 lines)
**The Converter Implementation Guide**

**Purpose:** Learn how converters work and how to implement new ones.

**Key Sections:**
1. Quick Reference - Converter table (60+ projects)
2. Project Structure Patterns - Shared code pattern
3. Revit Converter - Detailed example (2020-2025)
4. CSI Converter - Structural analysis pattern
5. Rhino/Grasshopper - CAD converter notes
6. Testing Pattern - Unit test examples
7. Common Pitfalls - Issues and solutions
8. Quick Start - New converter checklist
9. Reference - Conditional compilation symbols

**Read this when you need to:**
- Implement a new converter
- Add support for a new application version
- Understand bidirectional conversion
- Debug converter loading issues
- Extend existing converters
- Understand element conversion patterns

**Contains:**
- Full ConverterRevit example (main class + partial classes)
- Full ConverterCSI example (with Element1D conversion)
- Extension pattern examples
- Parameter handling in Revit
- CSI API integration patterns
- 10+ code examples
- Testing patterns

---

### 3. **DOCUMENTATION_SUMMARY.md** (12 KB, 330 lines)
**The Quick Start and Navigation Guide**

**Purpose:** Get oriented quickly and understand how to use the documentation.

**Key Sections:**
1. Files Overview - What each document contains
2. Core Findings - Architecture diagrams
3. Converter Pattern - Implementation structure
4. Key Patterns - Common design patterns
5. CSIBridge Context - Specific to your work
6. Quick Reference - File locations
7. Documentation Structure - When to use each doc
8. Next Steps - For CSIBridge development
9. Statistics - Size and scope

**Read this when you:**
- First approach the documentation
- Need a quick overview
- Want to find specific information
- Need context about CSIBridge work
- Want statistics and scope

**Contains:**
- Architecture diagrams
- Pattern summaries
- File location reference
- Navigation guide
- 15+ code pattern examples

---

## How to Use This Documentation

### For Learning the System
1. Start with **DOCUMENTATION_SUMMARY.md** Section 2 (Core Findings)
2. Read **OBJECTS.md** Section 1-3 (Overview and domain model)
3. Read **CONVERTERS.md** Section 1-2 (Quick reference and patterns)
4. Study real examples in **CONVERTERS.md** Section 2-3

### For Adding New Object Types
1. Go to **OBJECTS.md** Section 8
2. Follow the 8-step guide
3. Reference **CONVERTERS.md** Section 2 for converter template
4. Look at existing types in Objects/Objects/ directory

### For Implementing a New Converter
1. Start with **CONVERTERS.md** Section 1 (Quick reference)
2. Review Section 2 (Project structure)
3. Choose similar converter example (Revit or CSI)
4. Copy and customize the structure
5. Reference **DOCUMENTATION_SUMMARY.md** for patterns

### For CSIBridge Development
1. Read **DOCUMENTATION_SUMMARY.md** Section "CSIBridge Connector Context"
2. Study **CONVERTERS.md** Section 3 (CSI Converter details)
3. Look at Element1D/Element2D conversion examples
4. Reference conditional compilation patterns

### For Debugging Converter Issues
1. Check **CONVERTERS.md** Section 6 (Common Pitfalls)
2. Review **OBJECTS.md** Section 7 (Converter Discovery)
3. Check ObjectsKit.cs for version validation logic
4. Verify DLL naming: Objects.Converter.{AppName}.dll

---

## Key Concepts Reference

### Objects Kit
- **Type:** Default ISpeckleKit for Speckle
- **Location:** /Objects/Objects/ObjectsKit.cs
- **Purpose:** Enumerate types and discover converters
- **Key Method:** LoadConverter(string app)

### Converters
- **Interface:** ISpeckleConverter (Speckle.Core.Kits)
- **Pattern:** Shared code + version-specific wrappers
- **Discovery:** DLL scanning with version validation
- **Naming:** Objects.Converter.{AppName}.dll

### Domain Model
- **Categories:** 8 (BuiltElements, Geometry, Structural, etc.)
- **Total Types:** 200+
- **Organization:** Discipline-based hierarchies
- **Extending:** Add new class + app-specific subclass

### Conversion Directions
- **ToSpeckle:** Native format → Speckle objects (Import/Send)
- **ToNative:** Speckle objects → Native format (Export/Receive)
- **Pattern:** Switch statements or pattern matching
- **Context:** Document, objects, and settings

---

## Cross-Reference Guide

| Topic | Document | Section |
|-------|----------|---------|
| Objects overview | OBJECTS.md | 1 |
| Object types (200+) | OBJECTS.md | 2 |
| ObjectsKit class | OBJECTS.md | 3 |
| Converter discovery | OBJECTS.md | 7 |
| Adding new objects | OBJECTS.md | 8 |
| Converter pattern | CONVERTERS.md | 1-2 |
| Revit example | CONVERTERS.md | 2 |
| CSI example | CONVERTERS.md | 3 |
| Testing | CONVERTERS.md | 5 |
| Architecture diagram | DOCUMENTATION_SUMMARY.md | 2 |
| Design patterns | DOCUMENTATION_SUMMARY.md | 3 |
| CSIBridge context | DOCUMENTATION_SUMMARY.md | 4 |
| File locations | DOCUMENTATION_SUMMARY.md | 5 |

---

## File Structure

```
speckle-sharp-main/
├── OBJECTS.md                      # Domain model reference (34 KB)
├── CONVERTERS.md                   # Implementation guide (36 KB)
├── DOCUMENTATION_SUMMARY.md        # Quick start guide (12 KB)
├── DOCUMENTATION_INDEX.md          # This file
│
├── Objects/Objects/                # Core domain models
│   ├── BuiltElements/              # BIM objects (37 types + variants)
│   ├── Geometry/                   # Geometric primitives (20+ types)
│   ├── Structural/                 # Analysis objects
│   ├── Organization/               # Project structure
│   ├── Other/                      # Utilities
│   ├── GIS/                        # Geographic data
│   ├── Primitive/                  # Low-level types
│   ├── ObjectsKit.cs               # Kit implementation
│   └── Interfaces.cs               # Standard interfaces
│
└── Objects/Converters/             # Converter implementations
    ├── ConverterRevit/             # Revit 2020-2025 (80+ partial classes)
    ├── ConverterCSI/               # ETABS, SAP2000, Bridge, SAFE
    ├── ConverterRhinoGh/           # Rhino 6-8, Grasshopper 6-8
    ├── ConverterAutocadCivil/      # AutoCAD, Civil3D 2021-2025
    ├── ConverterDynamo/            # Dynamo for Revit
    ├── ConverterDxf/               # DXF geometry export
    ├── ConverterTeklaStructures/   # Tekla 2020-2023
    ├── ConverterNavisworks/        # Navisworks 2020-2025
    └── ConverterBentley/           # Bentley suite
```

---

## Statistics

| Metric | Count |
|--------|-------|
| Documentation files | 3 |
| Total documentation lines | 2,410 |
| Total documentation size | 82 KB |
| Object types | 200+ |
| Converter projects | 60+ |
| Application families | 10 |
| Revit converter partial classes | 80+ |
| CSI shared converter files | 57 |
| Code examples | 15+ |

---

## Version Information

- **Documentation Generated:** 2025-11-06
- **Repository:** speckle-sharp-main
- **Current Branch:** feature/csibridge-connector
- **Objects Package:** Speckle.Objects (netstandard2.0)
- **Target Framework:** .NET Standard 2.0

---

## Support & Context

These documents are designed to support:

1. **New developers** understanding Speckle Objects architecture
2. **Converter developers** implementing new application support
3. **Domain experts** extending object models
4. **CSIBridge connector** development (current focus)
5. **Maintenance teams** debugging and fixing converters
6. **Architecture reviews** understanding design patterns

---

## How to Navigate

**Start Here:**
- New to Speckle Objects? → DOCUMENTATION_SUMMARY.md
- Working on CSIBridge? → DOCUMENTATION_SUMMARY.md Section 4
- Building a converter? → CONVERTERS.md Section 2

**Deep Dives:**
- Understanding objects? → OBJECTS.md
- Implementing converters? → CONVERTERS.md

**Troubleshooting:**
- Converter not loading? → OBJECTS.md Section 7
- Conversion failing? → CONVERTERS.md Section 6
- Need patterns? → DOCUMENTATION_SUMMARY.md Section 3

---

**Last Updated:** 2025-11-06
**Scope:** Comprehensive analysis of Objects directory for creating detailed documentation
**Status:** Complete - Ready for use

