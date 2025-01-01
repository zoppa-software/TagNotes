# TagNotes
メモを時系列に管理し、単語、ハッシュタグで検索できるアプリケーション。  
  
## 特徴
<!--
- このプロジェクトは何をするものか？
- なぜこのプロジェクトが必要なのか？
- 主な機能は何か？
-->
※ **学習目的で実装しているため、実用向けのテストを行っていません。**
* メモを時系列に管理し、単語、ハッシュタグで検索できる  
* メモに埋め込んだパス、URLをクリックするとエクスプローラーでフォルダを開く、ブラウザで開くことができる  
* メモに設定した時刻になると通知する  
## 依存関係
フレームワークは`NET8`、UIは`WinUI3`で実装しています。  
最小フレームワークは`net8.0-windows10.0.19041.0`です。
## インストール方法
exeをコピーして配布したいため、パッケージ化をサポートしていません。  
`Release`ビルドしたファイルを配布してください。  
## 使い方
## サンプル
## ライセンス
MIT Licenses  
## 貢献
貢献方法は以下の通りです。
1. フォークする  
1. 新しいブランチを作成する(git checkout -b feature/YourFeature)  
1. コードをコミットする(git commit -m 'Add some feature')  
1. プッシュする(git push origin feature/YourFeature)  
1. プルリクエストを作成する  
## クレジット  
使用ライブラリ  
* [System.Data.SQLite](https://system.data.sqlite.org/)
* [ZoppaDSqlMapper](https://github.com/zoppa-software/ZoppaDSqlMapper)