namespace Olo.Bakery

type Runtime =
    | Windows64Bit
    | Linux64Bit

module Runtime =
    let toString runtime =
        match runtime with
        | Windows64Bit -> "win-x64"
        | Linux64Bit -> "linux-x64"
