プリセット定義してプロジェクト設定を切替可能にするツール

# 概要

## 対象としている課題

Unityのプロジェクト設定、BuildSettingsやPlayerSettingsなどは、対象デバイスや環境切替、作業ごとに切り替えたくなることが多々あります。
どんな設定を使うかの管理はプロジェクトごとの専用のスクリプトを定義したり、手動で切り替えていたりが多いように思います。
しかし、これだと属人性が高くなったり、オペレーションをミスしやすくなったりというペインがあります。

## 解決策

プロジェクト設定は設定ファイルにより宣言的にして、設定ファイルを選ぶことで丸ごと設定切替できるようにします。

# 使いかた

## インストール

* upm

  * Unity Editorで Window/Package Manager を開き、Add package from git URLで以下をインストールします。

  * ```
    git@github.com:uisawara/UnitySettingsConfigurator.git
    ```

## 基本

### プリセットの準備

- Project Windowで /Assets/Settings/で メニューからCreate -> Build -> Settingsで設定ファイルを作成します。
  - ![image-20241101135052180](./README.ja.assets/image-20241101135052180.png)
- 設定ファイルに設定を記述します。
- Unity Editorで Build/Build Configurator を選択してBuild Configurator Windowを表示します。すると定義したBuild Settingsが表示される筈です。
  - ![image-20241101135231904](./README.ja.assets/image-20241101135231904.png)
- 使いたい設定をクリックするとプレビューが表示されます。
- "Apply to Current Editor Settings" をクリックすると実際のエディタ設定が一括で切り替わります。

# 補足

* このPackageは積極的にChatGPT等を利用して作成されています。
* 改変したい場合はupmを通さずプロジェクトに直接ファイルを入れていただくか、git submoduleでの組み込みをするのがいいと思います。
