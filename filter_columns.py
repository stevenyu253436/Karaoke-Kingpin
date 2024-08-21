import pandas as pd

# 讀取Excel文件
input_file = 'SongsExport.xlsx'  # 替換為你的文件名
output_file = 'SongsExport.xlsx'  # 替換為你希望保存的文件名

# 使用pandas讀取Excel文件
df = pd.read_excel(input_file)

# 保留第一列和最後三列
columns_to_keep = [df.columns[0]] + df.columns[-3:].tolist()
df_filtered = df[columns_to_keep]

# 將結果保存到新的Excel文件
df_filtered.to_excel(output_file, index=False)

print(f"處理完畢，已保存到 {output_file}")
