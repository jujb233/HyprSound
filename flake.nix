{
  description = ".NET";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable"; # 可替换为其他版本
  };

  outputs =
    { self, nixpkgs }:
    let
      # 需要支持的架构列表
      systems = [ "x86_64-linux" ];
      lib = nixpkgs.lib;
    in
    {
      # 为每个系统生成 devShells.default
      devShells = lib.genAttrs systems (
        system:
        let
          pkgs = nixpkgs.legacyPackages.${system};

          dotnetPkg = pkgs.dotnetCorePackages.combinePackages [
            pkgs.dotnetCorePackages.sdk_10_0-bin
          ];

          deps = [
            dotnetPkg
          ];

          extraLibs = [ pkgs.sdl3 ] ++ deps;
        in
        {
          default = pkgs.mkShell {
            packages = [ pkgs.git ] ++ deps;

            LD_LIBRARY_PATH = pkgs.lib.makeLibraryPath extraLibs;

            shellHook = ''
              DOTNET_ROOT="${dotnetPkg}"
              DOTNET_CLI_TELEMETRY_OPTOUT=1
            '';
          };
        }
      );
    };
}
