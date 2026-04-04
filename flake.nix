{
  description = ".NET with NativeAOT and Rider support";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
  };

  outputs =
    { self, nixpkgs }:
    let
      systems = [ "x86_64-linux" ];
      lib = nixpkgs.lib;
    in
    {
      # 开发环境：包含 .NET SDK、native 依赖以及可选 Rider
      devShells = lib.genAttrs systems (
        system:
        let
          pkgs = nixpkgs.legacyPackages.${system};
          dotnetSdk = pkgs.dotnetCorePackages.sdk_10_0; # 使用非 -bin 版本，保证与 nixpkgs 集成
          # NativeAOT 所需的 native 依赖
          nativeDeps = with pkgs; [
            zlib
            openssl
            stdenv.cc.cc
            systemd
          ];
          # 额外工具（可选）
          tools = with pkgs; [ git ];
        in
        {
          default = pkgs.mkShell {
            packages = [
              dotnetSdk
              pkgs.sdl3
            ]
            ++ nativeDeps
            ++ tools;

            # 设置链接器路径和库路径，使 NativeAOT 能够找到系统库
            NIX_LD = "${pkgs.stdenv.cc.libc_bin}/bin/ld.so";
            NIX_LD_LIBRARY_PATH = pkgs.lib.makeLibraryPath (
              [ pkgs.stdenv.cc.cc ] ++ nativeDeps ++ [ pkgs.sdl3 ]
            );

            # 其它环境变量
            LD_LIBRARY_PATH = pkgs.lib.makeLibraryPath [ pkgs.sdl3 pkgs.systemd ];
            DOTNET_ROOT = "${dotnetSdk}";
            DOTNET_CLI_TELEMETRY_OPTOUT = "1";

            shellHook = ''
              echo "🚀 .NET 10 NativeAOT dev environment ready"
              echo "   LD_LIBRARY_PATH  : $LD_LIBRARY_PATH"
              echo "   NIX_LD_LIBRARY_PATH: $NIX_LD_LIBRARY_PATH"
            '';
          };
        }
      );

      # 可选：提供一个独立运行 Rider 的 FHS 环境（在 NixOS 上更稳定）
      apps = lib.genAttrs systems (system: {
        rider-env = {
          type = "app";
          program =
            let
              pkgs = nixpkgs.legacyPackages.${system};
              riderFHS = pkgs.buildFHSEnv {
                name = "rider-env";
                targetPkgs =
                  pkgs: with pkgs; [
                    dotnetCorePackages.sdk_10_0
                    dotnetCorePackages.aspnetcore_10_0
                    jetbrains.rider
                    powershell
                  ];
                runScript = "rider";
              };
            in
            "${riderFHS}/bin/rider-env";
        };
      });

      # 构建 .NET 应用（启用 NativeAOT）
      packages = lib.genAttrs systems (
        system:
        let
          pkgs = nixpkgs.legacyPackages.${system};
          dotnetSdk = pkgs.dotnetCorePackages.sdk_10_0;
          dotnetRuntime = dotnetSdk; # 使用同一版本的运行时
          nativeDeps = with pkgs; [
            zlib
            openssl
            stdenv.cc.cc
            sdl3
            systemd
            binutils
            glibc
          ];
        in
        {
          hyprsound = pkgs.buildDotnetModule {
            pname = "hyprsound";
            version = "0.0.1";

            src = ./.;
            projectFile = "HyprSound/HyprSound.csproj";

            dotnet-sdk = dotnetSdk;
            dotnet-runtime = dotnetRuntime;

            nugetDeps = ./deps.json;

            # NativeAOT 发布设置
            dotnetPublish = true;
            selfContainedBuild = true;
            dotnetPublishFlags = [
              "-c Release"
              "-p:LinkerFlavor=gcc"
            ];

            # 构建时需要这些 native 依赖（链接阶段）
            buildInputs = nativeDeps;

            preBuild = ''
              export LIBRARY_PATH="${pkgs.glibc}/lib:$LIBRARY_PATH"
              export CPATH="${pkgs.glibc}/include:$CPATH"
            '';

            # 安装后包装可执行文件，确保运行时能找到 SDL3
            postInstall = ''
              mkdir -p $out/bin
              cp $out/lib/hyprsound/HyprSound $out/bin/hyprsound
              wrapProgram $out/bin/hyprsound \
                --set LD_LIBRARY_PATH "${pkgs.lib.makeLibraryPath [ pkgs.sdl3 pkgs.systemd ]}"
            '';

            meta = with pkgs.lib; {
              description = "HyprSound - A .NET AOT application using SDL3";
              platforms = platforms.linux;
            };
          };
        }
      );
    };
}
