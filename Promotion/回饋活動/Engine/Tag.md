根據PromotionRecord標記tag在商品上



var matchedPromotionTag = $"$matched_{record.SourceType.ToString().ToLower()}:{record.PromotionRuleId}";

把Tags標在PurchasedItem的Flags上