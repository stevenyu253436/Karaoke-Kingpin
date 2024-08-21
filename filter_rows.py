import pandas as pd

# 讀取Excel文件
input_file = 'SongsExport.xlsx'  # 替換為你的文件名
output_file = 'SongsExport.xlsx'  # 替換為你希望保存的文件名

# 使用pandas讀取Excel文件
df = pd.read_excel(input_file)

# 保留第一行和最後三行
rows_to_keep = pd.concat([df.head(1), df.tail(3)], ignore_index=True)

# 將結果保存到新的Excel文件
rows_to_keep.to_excel(output_file, index=False)

print(f"處理完畢，已保存到 {output_file}")
