# UnityOneWeek_CoinCat_CodeSample

Unity1Week参加作品「つながれ!コインキャット!」の、自作コードを抜粋したポートフォリオ用リポジトリです。

## 作品概要

「つながれ!コインキャット!」は、Unity1Weekのお題に沿って制作・公開したUnity製ゲームです。

短期間の制作において、ゲームルールの設計、Unity / C#による実装、UIや演出の調整、公開までを一貫して行いました。

作品URL：  
https://unityroom.com/games/coincatconnect

## 掲載内容

### Game

ゲーム本編に関するコードです。

- アバター制御
- コイン制御
- ゲーム進行管理
- UI制御
- 背景演出
- クリア演出

### Title

タイトル画面に関するコードです。

- タイトル画面の進行管理
- ロゴ演出
- 隠し効果音機能

## フォルダ構成

```text
UnityOneWeek_CoinCat_CodeSample
├─ Game
│  ├─ Avatar.cs
│  ├─ BackgroundMover.cs
│  ├─ Bank.cs
│  ├─ ClearAnim.cs
│  ├─ Coin.cs
│  ├─ GameBoard.cs
│  ├─ GameSceneController.cs
│  └─ GameSceneUI.cs
├─ Title
│  ├─ TitleLogoAnimator.cs
│  ├─ TitleSceneController.cs
│  └─ TitleSoundTrigger.cs
└─ README.md
