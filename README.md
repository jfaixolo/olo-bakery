# Bakery

Bakery is a library of helper functions for creating [Cake.Frosting](https://cakebuild.net/docs/running-builds/runners/cake-frosting) build scripts.

## Repository structure

- `/src`
  - `/Olo.Bakery` - Library source code
- `/example`
  - `/Example.App` - Hello World demo app
  - `/_build` - Example build using library

## Assumptions

- Consuming projects use top-level `/src` and `/tests` folders
- Consuming projects have Octopus dotnet tool installed (if using package target)

## Limitations

Cake.Frosting uses reflection to discover build targets in the startup project. Because of this, target classes cannot be reused, but most of the logic can be.
