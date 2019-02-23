# MySomatosensory

DetectManager.cs：
###### 是否通過、正確率、門檻值、標準模型、比對關節，全都透過 enum DetectSkeleton 當作KEY去抓取。
###### 目前可做到利用一個函式compareMovement判斷所有動作，且同時偵測多種動作，像單腳跳需要比對正確率以及跳躍高度這兩個條件，也可利用additional_condition 紀錄跳躍高度等，每個分解動作的正確率與門檻成功或失敗都記錄，additional_condition 目前還沒寫到紀錄。
###### 因為研究生的遊戲會使用同時判斷多種動作，因此我也寫了多種動作皆失敗的處理方法

GameInfo.cs：
###### 靜態類別，可用於不同腳本間傳遞變數，我用來記錄玩家ID等資料

Main.cs：
###### 我用來測試的腳本

RePlay.cs：
###### 因為紀錄資料中新增了旋轉角度，現在重播可直接用模型，我覺得效果比用sphere好
