# MySomatosensory

DetectManager.cs：
* 是否通過、正確率、門檻值、標準模型、比對關節，全都透過 enum DetectSkeleton 當作KEY去抓取。

* 目前可做到利用一個函式compareMovement判斷所有動作，且同時偵測多種動作，像單腳跳需要比對正確率以及跳躍高度這兩個條件，也可利用additional_condition 紀錄跳躍高度等，每個分解動作的正確率與門檻成功或失敗都記錄，additional_condition 目前還沒寫到紀錄。

* 因為研究生的遊戲會使用同時判斷多種動作，因此我也寫了多種動作皆失敗的處理方法

GameInfo.cs：

* 靜態類別，可用於不同腳本間傳遞變數，我用來記錄玩家ID等資料

Main.cs：

* 我用來測試的腳本

RePlay.cs：

* 因為紀錄資料中新增了旋轉角度，現在重播可直接用模型，我覺得效果比用sphere好

RecordData.cs
* public string save(string file_id)，利用時間戳 file_id 區分檔案，並回傳路徑


* public static void finishWriting(string path)
// 一個遊戲的所有紀錄皆寫完後，加上後綴"_done"，告訴其他程式已經可以上傳

## 開發注意事項

* 發生過多次一執行場景，Unity 沒有出現錯誤訊息就直接關閉，查看 Console 當中的 Editor log 也未發現什麼錯誤，結果就是深度攝影機忘記接上，Nuitrack 無法獲取攝影機導致。
* 逐步降低學習率。
* 模型亂跳問題(本身的 animator 不要勾或不要勾它的 apply root motion，改勾 AvatarController 的 external root motion，使用其他物件或腳本來控制模型的位置)。
* 需要偵測前後移動時，該如何放寬，讓他可以前後移動？→設置另一個可自由移動的模型於視野外側，偵測它的動作，由視野內的模型執行結果。
* API 用 4.x 的版本。
* 移動的自然程度→動畫匯入。
* 時進的調整，不要一瞬間就跳到下一步驟。
* 偵測的停止不要讓玩家發現。
* UI的美化。
