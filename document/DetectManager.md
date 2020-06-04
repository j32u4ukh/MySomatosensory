# DetectManager

提供動作比對的核心演算法，透過 delegate 讓外部腳本根據自身需求，定義比對的動作、以及額外正確率的傳入。

![detectManager](/document/image/detectManager.png)

`實際在迴圈中進行偵測的函式是 detectManager ，DetectManager 當中提供實際的 Player 給函式來進行比對`

![setDetectDelegate](/document/image/setDetectDelegate.png)

`透過 setDetectDelegate 將 detectManager 指向外部定義的函式`

![detectRaiseTwoHands](/document/image/detectRaiseTwoHands.png)

`detectRaiseTwoHands 以 Player 為參數，取得要比對的動作(Pose)後進行客製化的比對`

同時也提供了關節數據紀錄，只需告訴 DetectManager 何時開始記錄，何時結束即可。

## 偵測原理

將一個動作視為多個分解動作，進行多次的姿勢配對。

![圖片代替文字](/document/image/ActionDecomposition.png)

### 姿勢配對

由深度感測器所捕捉到的關節點位置(以下稱玩家關節點)，會透過 Kinect 的工具轉換成模型的關節點位置，利用這些關節點和事先決定好的關節點(以下稱比對標準關節點)進行比較。

![圖片代替文字](/document/image/CompareSkeleton.png)

事先決定好的關節點可以是從調整過姿勢的模型取得，也可以將關節點的位置數據儲存，需要的時候再讀取進來。

**比較方式**

分別從玩家關節點和比對標準關節點各取兩個關節點(玩家 2 個，比對標準 2 個)，各自形成一條三維的[向量](https://zh.wikipedia.org/wiki/%E5%90%91%E9%87%8F)(x, y, z)，須轉換為單位向量。

![圖片代替文字](/document/image/vector.png)

透過計算兩個向量的[內積](https://zh.wikipedia.org/wiki/%E7%82%B9%E7%A7%AF)，進一步取得兩向量之間的夾角。夾角越小，表示兩個向量越接近。

![圖片代替文字](/document/image/DotProduct.png)

這裡沿用 Kinect 原始的姿勢比對，將角度轉換為 0 ~ 1 的數值，差異最大為 90 度，因此超過 90 度都換轉換為 1。又，我們需要的是正確率，因此會再用 1 減掉該數值。

第一點和第二點形成一條向量，第二點和第三點形成一條向量，以此類推，而最後一點會和第一點再形成一條向量。

## 數據載入

DetectManager 負責載入在動作比對過程中，不會因人而異的部分，如各個動作**要比對的關節**，以及**比對的標準**。

### 要比對的關節

前面提到要兩兩一組，形成向量的關節點。

要比對的關節自行獨立為一個檔案，且大小較小，因此採用一次將所有數據一起讀入的方式。

### 比對的標準

每個分解動作的關節點位置。

比對的標準根據事前準備的數量，各個動作有所不同，檔案大小也較大，因此採需要前再載入。