
## 11. Apple Pay 的憑證

Apple Pay 憑證（Apple Pay Merchant Certificate），是由 Apple 核發給「商店（商家）」的一個「身分認證」數位憑證。一份讓你的 App（或網站）可以合法呼叫 Apple Pay 並取得付款 token 的「身份證 + 安全通行證」

<br>

🍎 Apple Pay 的核心原則是：

<br>

讓商家永遠無法碰到消費者卡號 → 透過 token + 加密 + 憑證，完全封裝金流路徑

<br>

Apple 不相信你（也不信 PSP）會做好風控，所以整個流程設計成：

<br>

Apple 保管卡號

<br>

Apple Pay Token 是一次性、短效的

<br>

憑證加密 token → 只能給特定 PSP 解開

<br>

憑證就是用來「授權解密」的唯一身分識別

<br>
