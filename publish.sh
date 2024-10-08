#!/bin/bash

set -e

# 現在のディレクトリを保存
ORIGINAL_DIR=$(pwd)
# 現在のスクリプトのディレクトリに移動
cd "$(dirname "$0")"

# 初期化
rm -rf ./bin/Package

# ビルド
dotnet build -c Release -o ./bin/Package

# コピー
cp -r ./Yak/manifest.yml ./bin/Package
cp -r ./resources/MyCustomTools.rui ./bin/Package

# パッケージング
cd ./bin/Package
"/Applications/Rhino 8.app/Contents/Resources/bin/yak" build

# 生成されたyakファイルの名前を取得
YAK_FILE=$(find . -maxdepth 1 -name "*.yak" -print -quit)

if [ -z "$YAK_FILE" ]; then
    echo "No .yak file found."
    exit 1
fi

# パッケージをアップロード
"/Applications/Rhino 8.app/Contents/Resources/bin/yak" push "$YAK_FILE"

# 元のディレクトリに戻る
cd "$ORIGINAL_DIR"