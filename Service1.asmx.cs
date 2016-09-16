using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Web;
using System.Web.Services;
using Oracle.DataAccess.Client;

namespace is2goirai
{
	/// <summary>
	/// [is2goirai]
	/// </summary>
	//--------------------------------------------------------------------------
	// 修正履歴
	//--------------------------------------------------------------------------
	// ADD 2007.04.28 東都）高木 オブジェクトの破棄
	//	disposeReader(reader);
	//	reader = null;
	// DEL 2007.05.10 東都）高木 未使用関数のコメント化
	//	logFileOpen(sUser);
	//	userCheck2(conn2, sUser);
	//	logFileClose();
	//--------------------------------------------------------------------------
	// MOD 2008.09.18 東都）高木 一覧のソート順に[荷送人ＣＤ]を追加 
	// MOD 2008.10.01 東都）高木 一覧に請求先を表示 
	//--------------------------------------------------------------------------
	// MOD 2009.11.30 東都）高木 検索条件に名前、請求先を追加 
	//--------------------------------------------------------------------------
	// MOD 2010.09.08 東都）高木 ＣＳＶ出力機能の追加 
	// MOD 2010.09.08 東都）高木 ＣＳＶ取込機能の追加 
	// MOD 2010.09.17 東都）高木 複数件削除機能の追加 
	// MOD 2010.09.27 東都）高木 請求先部課名の追加 
	// MOD 2010.09.29 東都）高木 郵便番号(__)対応（＊既存バグだが導入） 
	//--------------------------------------------------------------------------
	// MOD 2011.01.18 東都）高木 複数件削除機能の障害対応 
	// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない 
	// MOD 2011.03.09 東都）高木 請求先マスタの主キーに[会員ＣＤ]を追加 
	// MOD 2011.04.13 東都）高木 重量入力不可対応 
	// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 
	//--------------------------------------------------------------------------
	// MOD 2012.09.06 COA)横山 Oracleサーバ負荷軽減対策（SQLにバインド変数を利用）
	//--------------------------------------------------------------------------

	[System.Web.Services.WebService(
		 Namespace="http://Walkthrough/XmlWebServices/",
		 Description="is2goirai")]

	public class Service1 : is2common.CommService
	{
		private static string sSepa = "|";
// MOD 2010.09.08 東都）高木 ＣＳＶ出力機能の追加 START
//		private static string sCRLF = "\\r\\n";
		private static string sKanma = ",";
		private static string sDbl = "\"";
		private static string sSng = "'";
// MOD 2010.09.08 東都）高木 ＣＳＶ出力機能の追加 END

		public Service1()
		{
			//CODEGEN: この呼び出しは、ASP.NET Web サービス デザイナで必要です。
			InitializeComponent();

			connectService();
		}

		#region コンポーネント デザイナで生成されたコード 
		
		//Web サービス デザイナで必要です。
		private IContainer components = null;
				
		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// 使用されているリソースに後処理を実行します。
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if(disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);		
		}
		
		#endregion

		/*********************************************************************
		 * 依頼主一覧取得
		 * 引数：会員ＣＤ、部門ＣＤ、カナ、荷送人ＣＤ
		 * 戻値：ステータス、一覧（名前１、住所１、荷送人ＣＤ、電話番号、カナ略称）
		 *
		 *********************************************************************/
// MOD 2005.05.11 東都）高木 ORA-03113対策？ START
//		private static string GET_GOIRAI_SELECT
//			= "SELECT 名前１,住所１,荷送人ＣＤ,'(' || TRIM(電話番号１) || ')' \n"
//			+       " || TRIM(電話番号２) || '-' || TRIM(電話番号３),カナ略称 \n"
//			+ " FROM ＳＭ０１荷送人 \n";
// MOD 2008.10.01 東都）高木 一覧に請求先を表示 START
//		private static string GET_GOIRAI_SELECT
//			= "SELECT 名前１, 住所１, 荷送人ＣＤ, 電話番号１, \n"
//			+       " 電話番号２, 電話番号３, カナ略称 \n"
//			+ " FROM ＳＭ０１荷送人 \n";
		private static string GET_GOIRAI_SELECT_1
			= "SELECT SM01W.名前１, SM01W.住所１, SM01W.荷送人ＣＤ, SM01W.電話番号１ \n"
			+      ", SM01W.電話番号２, SM01W.電話番号３, SM01W.カナ略称 \n"
			+      ", NVL(SM04.得意先部課名, SM01W.郵便番号 || ' ' || SM01W.得意先ＣＤ || ' ' || SM01W.得意先部課ＣＤ) \n"
// MOD 2009.11.30 東都）高木 検索条件に名前、請求先を追加 START
			+      ", SM01W.名前２, SM01W.登録日時, SM01W.更新日時 \n"
// MOD 2009.11.30 東都）高木 検索条件に名前、請求先を追加 END
			+  " FROM ( \n"
			+      "SELECT SM01.名前１, SM01.住所１, SM01.荷送人ＣＤ, SM01.電話番号１ \n"
			+      ", SM01.電話番号２, SM01.電話番号３, SM01.カナ略称 \n"
			+      ", SM01.得意先ＣＤ, SM01.得意先部課ＣＤ \n"
			+      ", CM02.郵便番号 \n"
// MOD 2009.11.30 東都）高木 検索条件に名前、請求先を追加 START
			+      ", SM01.名前２, SM01.登録日時, SM01.更新日時 \n"
// MOD 2009.11.30 東都）高木 検索条件に名前、請求先を追加 END
// MOD 2011.03.09 東都）高木 請求先マスタの主キーに[会員ＣＤ]を追加 START
			+      ", SM01.会員ＣＤ \n"
// MOD 2011.03.09 東都）高木 請求先マスタの主キーに[会員ＣＤ]を追加 END
			+ " FROM ＳＭ０１荷送人 SM01 \n"
			+     ", ＣＭ０２部門   CM02 \n"
			;
		private static string GET_GOIRAI_SELECT_2
			=     ") SM01W \n"
			+     ", ＳＭ０４請求先 SM04 \n"
			+     " WHERE SM01W.郵便番号 = SM04.郵便番号(+) \n"
			+       " AND SM01W.得意先ＣＤ = SM04.得意先ＣＤ(+) \n"
			+       " AND SM01W.得意先部課ＣＤ = SM04.得意先部課ＣＤ(+) \n"
// MOD 2011.03.09 東都）高木 請求先マスタの主キーに[会員ＣＤ]を追加 START
			+       " AND SM01W.会員ＣＤ = SM04.会員ＣＤ(+) \n"
// MOD 2011.03.09 東都）高木 請求先マスタの主キーに[会員ＣＤ]を追加 END
			+       " AND '0' = SM04.削除ＦＧ(+) \n"
// MOD 2009.11.30 東都）高木 検索条件に名前、請求先を追加 START
//			+     " ORDER BY SM01W.名前１, SM01W.荷送人ＣＤ \n"
// MOD 2009.11.30 東都）高木 検索条件に名前、請求先を追加 END
			;
// MOD 2008.10.01 東都）高木 一覧に請求先を表示 END
// MOD 2005.05.11 東都）高木 ORA-03113対策？ END

		[WebMethod]
		public String[] Get_irainusi(string[] sUser, string sKCode, string sBCode, string sKana, string sICode)
		{
// MOD 2009.11.30 東都）高木 検索条件に名前、請求先を追加 START
			string[] sKey = new string[]{
				sKCode, sBCode, sKana, sICode
			};
			return Get_irainusi2(sUser, sKey);
		}
		
		[WebMethod]
		public String[] Get_irainusi2(string[] sUser, string[] sKey)
		{
			string sKCode  = "";
			string sBCode  = "";
			string sKana   = "";
			string sICode  = "";
			string s名前   = "";
			string s請求先 = "";
			string s部課   = "";
			int i階層リスト１ = 0;
			int iソート方向１ = 0;
			int i階層リスト２ = 0;
			int iソート方向２ = 0;

			sKCode = sKey[0];
			sBCode = sKey[1];
			sKana  = sKey[2];
			sICode = sKey[3];
			if(sKey.Length > 4){
				s名前   = sKey[4];
				s請求先 = sKey[5];
				s部課   = sKey[6];
				try{
					i階層リスト１ = int.Parse(sKey[7]);
					iソート方向１ = int.Parse(sKey[8]);
					i階層リスト２ = int.Parse(sKey[9]);
					iソート方向２ = int.Parse(sKey[10]);
				}catch(Exception){
					;
				}
			}
// MOD 2009.11.30 東都）高木 検索条件に名前、請求先を追加 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//			logFileOpen(sUser);
			logWriter(sUser, INF, "依頼主一覧取得開始");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//// ADD 2005.05.23 東都）小童谷 会員チェック追加 START
//			// 会員チェック
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 東都）小童谷 会員チェック追加 END

			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbRet = new StringBuilder(1024);
			try
			{
// MOD 2008.10.01 東都）高木 一覧に請求先を表示 START
//				sbQuery.Append(GET_GOIRAI_SELECT);
//				sbQuery.Append(" WHERE 会員ＣＤ = '" + sKCode + "' \n");
//				sbQuery.Append("   AND 部門ＣＤ = '" + sBCode + "' \n");
//
//				if(sKana.Length > 0 && sICode.Length == 0)
//				{
//					sbQuery.Append(" AND カナ略称 LIKE '"+ sKana + "%' \n");
//				}
//				if(sICode.Length > 0 && sKana.Length == 0)
//				{
//					sbQuery.Append(" AND 荷送人ＣＤ LIKE '"+ sICode + "%' \n");
//				}
//				if(sICode.Length > 0 && sKana.Length > 0)
//				{
//					sbQuery.Append(" AND カナ略称 LIKE '"+ sKana + "%' \n");
//					sbQuery.Append(" AND 荷送人ＣＤ LIKE '"+ sICode + "%' \n");
//				}
//				sbQuery.Append(" AND 削除ＦＧ = '0' \n");
//// MOD 2008.09.18 東都）高木 一覧のソート順に[荷送人ＣＤ]を追加 START
////				sbQuery.Append(" ORDER BY 名前１ \n");
//				sbQuery.Append(" ORDER BY 名前１, 荷送人ＣＤ \n");
//// MOD 2008.09.18 東都）高木 一覧のソート順に[荷送人ＣＤ]を追加 END
				sbQuery.Append(GET_GOIRAI_SELECT_1);
				sbQuery.Append(" WHERE SM01.会員ＣＤ = '" + sKCode + "' \n");
				sbQuery.Append("   AND SM01.部門ＣＤ = '" + sBCode + "' \n");
				sbQuery.Append("   AND SM01.会員ＣＤ = CM02.会員ＣＤ \n");
				sbQuery.Append("   AND SM01.部門ＣＤ = CM02.部門ＣＤ \n");

				if(sKana.Length > 0 && sICode.Length == 0)
				{
					sbQuery.Append(" AND SM01.カナ略称 LIKE '"+ sKana + "%' \n");
				}
				if(sICode.Length > 0 && sKana.Length == 0)
				{
					sbQuery.Append(" AND SM01.荷送人ＣＤ LIKE '"+ sICode + "%' \n");
				}
				if(sICode.Length > 0 && sKana.Length > 0)
				{
					sbQuery.Append(" AND SM01.カナ略称 LIKE '"+ sKana + "%' \n");
					sbQuery.Append(" AND SM01.荷送人ＣＤ LIKE '"+ sICode + "%' \n");
				}
// MOD 2009.11.30 東都）高木 検索条件に名前、請求先を追加 START
				if(s名前.Length > 0){
					sbQuery.Append(" AND SM01.名前１ LIKE '%"+ s名前 + "%' \n");
				}
				if(s請求先.Length > 0){
					sbQuery.Append(" AND SM01.得意先ＣＤ = '"+ s請求先 + "' \n");
					if(s部課.Length > 0){
						sbQuery.Append(" AND SM01.得意先部課ＣＤ = '"+ s部課 + "' \n");
					}else{
						sbQuery.Append(" AND SM01.得意先部課ＣＤ = ' ' \n");
					}
				}
// MOD 2009.11.30 東都）高木 検索条件に名前、請求先を追加 END
				sbQuery.Append(" AND SM01.削除ＦＧ = '0' \n");
				sbQuery.Append(GET_GOIRAI_SELECT_2);
// MOD 2008.10.01 東都）高木 一覧に請求先を表示 END

// MOD 2009.11.30 東都）高木 検索条件に名前、請求先を追加 START
				sbQuery.Append(" ORDER BY \n");
				switch(i階層リスト１){
				case 1:
					sbQuery.Append(" SM01W.カナ略称 ");
					if(iソート方向１ == 1) sbQuery.Append(" DESC ");
					break;
				case 2:
					sbQuery.Append(" SM01W.荷送人ＣＤ");
					if(iソート方向１ == 1) sbQuery.Append(" DESC ");
					break;
				case 3:
//					sbQuery.Append(" SM01W.得意先ＣＤ ");
//					if(iソート方向１ == 1) sbQuery.Append(" DESC ");
//					sbQuery.Append(", SM01W.得意先部課ＣＤ ");
					sbQuery.Append(" NVL(SM04.得意先部課名,SM01W.得意先ＣＤ || SM01W.得意先部課ＣＤ) ");
					if(iソート方向１ == 1) sbQuery.Append(" DESC ");
					break;
				case 4:
					sbQuery.Append(" SM01W.名前１ ");
					if(iソート方向１ == 1) sbQuery.Append(" DESC ");
//					sbQuery.Append(", SM01W.名前２ ");
//					if(iソート方向１ == 1) sbQuery.Append(" DESC ");
					break;
				case 5:
					sbQuery.Append(" SM01W.電話番号１ ");
					if(iソート方向１ == 1) sbQuery.Append(" DESC ");
					sbQuery.Append(", SM01W.電話番号２ ");
					if(iソート方向１ == 1) sbQuery.Append(" DESC ");
					sbQuery.Append(", SM01W.電話番号３ ");
					if(iソート方向１ == 1) sbQuery.Append(" DESC ");
					break;
				case 6:
					sbQuery.Append(" SM01W.登録日時 ");
					if(iソート方向１ == 1) sbQuery.Append(" DESC ");
					break;
				case 7:
					sbQuery.Append(" SM01W.更新日時 ");
					if(iソート方向１ == 1) sbQuery.Append(" DESC ");
					break;
				}
				if(i階層リスト１ != 0 && i階層リスト２ != 0){
					sbQuery.Append(",");
				}
				switch(i階層リスト２){
				case 1:
					sbQuery.Append(" SM01W.カナ略称 ");
					if(iソート方向２ == 1) sbQuery.Append(" DESC ");
					break;
				case 2:
					sbQuery.Append(" SM01W.荷送人ＣＤ");
					if(iソート方向２ == 1) sbQuery.Append(" DESC ");
					break;
				case 3:
//					sbQuery.Append(" SM01W.得意先ＣＤ ");
//					if(iソート方向２ == 1) sbQuery.Append(" DESC ");
//					sbQuery.Append(", SM01W.得意先部課ＣＤ ");
					sbQuery.Append(" NVL(SM04.得意先部課名,SM01W.得意先ＣＤ || SM01W.得意先部課ＣＤ) ");
					if(iソート方向２ == 1) sbQuery.Append(" DESC ");
					break;
				case 4:
					sbQuery.Append(" SM01W.名前１ ");
					if(iソート方向２ == 1) sbQuery.Append(" DESC ");
//					sbQuery.Append(", SM01W.名前２ ");
//					if(iソート方向２ == 1) sbQuery.Append(" DESC ");
					break;
				case 5:
					sbQuery.Append(" SM01W.電話番号１ ");
					if(iソート方向２ == 1) sbQuery.Append(" DESC ");
					sbQuery.Append(", SM01W.電話番号２ ");
					if(iソート方向２ == 1) sbQuery.Append(" DESC ");
					sbQuery.Append(", SM01W.電話番号３ ");
					if(iソート方向２ == 1) sbQuery.Append(" DESC ");
					break;
				case 6:
					sbQuery.Append(" SM01W.登録日時 ");
					if(iソート方向２ == 1) sbQuery.Append(" DESC ");
					break;
				case 7:
					sbQuery.Append(" SM01W.更新日時 ");
					if(iソート方向２ == 1) sbQuery.Append(" DESC ");
					break;
				}
				if(i階層リスト１ == 0 && i階層リスト２ == 0){
					sbQuery.Append(" SM01W.名前１ \n");
				}
				if(i階層リスト１ != 2 && i階層リスト２ != 2){
					sbQuery.Append(", SM01W.荷送人ＣＤ \n");
				}
// MOD 2009.11.30 東都）高木 検索条件に名前、請求先を追加 END
				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);

				while (reader.Read())
				{
					sbRet = new StringBuilder(1024);

// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない START
//					sbRet.Append(sSepa + reader.GetString(0).Trim());
//					sbRet.Append(sSepa + reader.GetString(1).Trim());
					sbRet.Append(sSepa + reader.GetString(0).TrimEnd()); // 名前１
					sbRet.Append(sSepa + reader.GetString(1).TrimEnd()); // 住所１
// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない END
					sbRet.Append(sSepa + reader.GetString(2).Trim());
// MOD 2005.05.11 東都）高木 ORA-03113対策？ START
//					sbRet.Append(sSepa + reader.GetString(3));
//					sbRet.Append(sSepa + reader.GetString(4).Trim());
					sbRet.Append(sSepa + "(" + reader.GetString(3).Trim() + ")"
										+ reader.GetString(4).Trim() + "-"
										+ reader.GetString(5).Trim());	// 電話番号
// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない START
//					sbRet.Append(sSepa + reader.GetString(6).Trim()); // カナ略称
					sbRet.Append(sSepa + reader.GetString(6).TrimEnd());// カナ略称
// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない END
// MOD 2005.05.11 東都）高木 ORA-03113対策？ END
// ADD 2008.10.01 東都）高木 一覧に請求先を表示 START
					sbRet.Append(sSepa + reader.GetString(7).Trim());	// 依頼主
// ADD 2008.10.01 東都）高木 一覧に請求先を表示 END

					sList.Add(sbRet);
				}
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
				sRet = new string[sList.Count + 1];

				if (sList.Count == 0)
					sRet[0] = "該当データがありません";
				else
				{
					sRet[0] = "正常終了";
					int iCnt = 1;
					IEnumerator enumList = sList.GetEnumerator();
					while(enumList.MoveNext())
					{
						sRet[iCnt] = enumList.Current.ToString();
						iCnt++;
					}
				}

				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				conn2 = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
			}
			
			return sRet;
		}

		/*********************************************************************
		 * 依頼主データ取得
		 * 引数：会員ＣＤ、部門ＣＤ、荷送人ＣＤ
		 * 戻値：ステータス、カナ略称、電話番号、郵便番号、住所、名前、重量
		 *		 メールアドレス、得意先ＣＤ、得意先部課ＣＤ、更新日時
		 *********************************************************************/
// MOD 2011.01.18 東都）高木 複数件削除機能の障害対応 START
//// ADD 2005.05.11 東都）高木 ORA-03113対策？ START
//		private static string GET_SIRAINUSI_SELECT
//			= "SELECT カナ略称, 電話番号１, 電話番号２, 電話番号３, 郵便番号, \n"
//			+ " 住所１, 住所２, 名前１, 名前２, 才数, \n"
//			+ " 重量, \"メールアドレス\", 得意先ＣＤ, 得意先部課ＣＤ, 更新日時 \n"
//			+ " FROM ＳＭ０１荷送人 \n";
//// ADD 2005.05.11 東都）高木 ORA-03113対策？ END
// MOD 2011.01.18 東都）高木 複数件削除機能の障害対応 END

		[WebMethod]
		public String[] Get_Sirainusi(string[] sUser, string sKCode,string sBCode,string sICode)
		{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//			logFileOpen(sUser);
			logWriter(sUser, INF, "依頼主情報取得開始");

			OracleConnection conn2 = null;
// MOD 2010.09.27 東都）高木 請求先部課名の追加 START
//			string[] sRet = new string[18];
			string[] sRet = new string[19];
// MOD 2010.09.27 東都）高木 請求先部課名の追加 END

			// ADD-S 2012.09.06 COA)横山 Oracleサーバ負荷軽減対策（SQLにバインド変数を利用）
			OracleParameter[]	wk_opOraParam	= null;
			// ADD-E 2012.09.06 COA)横山 Oracleサーバ負荷軽減対策（SQLにバインド変数を利用）

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//// ADD 2005.05.23 東都）小童谷 会員チェック追加 START
//			// 会員チェック
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 東都）小童谷 会員チェック追加 END

// MOD 2010.09.27 東都）高木 請求先部課名の追加 START
			string s会員ＣＤ   = sKCode;
			string s部門ＣＤ   = sBCode;
			string s依頼主ＣＤ = sICode;
			StringBuilder sbQuery = new StringBuilder(1024);
// MOD 2010.09.27 東都）高木 請求先部課名の追加 END
			try
			{
// MOD 2005.05.11 東都）高木 ORA-03113対策？ START
//				string cmdQuery = "SELECT カナ略称,電話番号１,電話番号２,電話番号３, \n"
//					+ " substr(郵便番号,1,3),substr(郵便番号,4,4),住所１,住所２,名前１,名前２, \n"
//					+ " TO_CHAR(才数),TO_CHAR(重量), \"メールアドレス\",得意先ＣＤ,得意先部課ＣＤ, \n"
//					+ " TO_CHAR(更新日時) \n"
//					+ " FROM ＳＭ０１荷送人 \n"
//					+ " WHERE 荷送人ＣＤ = '" + sICode + "' AND 会員ＣＤ = '" + sKCode + "' \n"
//					+  "  AND 部門ＣＤ   = '" + sBCode + "' AND 削除ＦＧ = '0'";
// MOD 2010.09.27 東都）高木 請求先部課名の追加 START
//				string cmdQuery = GET_SIRAINUSI_SELECT
//					+ " WHERE 荷送人ＣＤ = '" + sICode + "' \n"
//					+ " AND 会員ＣＤ = '" + sKCode + "' \n"
//					+ " AND 部門ＣＤ = '" + sBCode + "' \n"
//					+ " AND 削除ＦＧ = '0' \n";
//// MOD 2005.05.11 東都）高木 ORA-03113対策？ END
//				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
//
				sbQuery.Append("SELECT SM01.カナ略称 \n");
				sbQuery.Append(      ",SM01.電話番号１ \n");
				sbQuery.Append(      ",SM01.電話番号２ \n");
				sbQuery.Append(      ",SM01.電話番号３ \n");
				sbQuery.Append(      ",SM01.郵便番号 \n");
				sbQuery.Append(      ",SM01.住所１ \n");
				sbQuery.Append(      ",SM01.住所２ \n");
				sbQuery.Append(      ",SM01.名前１ \n");
				sbQuery.Append(      ",SM01.名前２ \n");
				sbQuery.Append(      ",SM01.才数 \n");
				sbQuery.Append(      ",SM01.重量 \n");
				sbQuery.Append(      ",SM01.\"メールアドレス\" \n");
				sbQuery.Append(      ",SM01.得意先ＣＤ \n");
				sbQuery.Append(      ",SM01.得意先部課ＣＤ \n");
				sbQuery.Append(      ",SM01.更新日時 \n");
				sbQuery.Append(      ",NVL(SM04.得意先部課名,' ') \n");
				sbQuery.Append( "FROM ＳＭ０１荷送人 SM01 \n");
				sbQuery.Append(" LEFT JOIN \"ＣＭ０２部門\" CM02 \n");
				sbQuery.Append(" ON SM01.会員ＣＤ = CM02.会員ＣＤ ");
				sbQuery.Append("AND SM01.部門ＣＤ = CM02.部門ＣＤ ");
				sbQuery.Append("AND CM02.削除ＦＧ = '0' \n");
				sbQuery.Append(" LEFT JOIN \"ＳＭ０４請求先\" SM04 \n");
				sbQuery.Append(" ON CM02.郵便番号 = SM04.郵便番号 ");
				sbQuery.Append("AND SM01.得意先ＣＤ = SM04.得意先ＣＤ ");
				sbQuery.Append("AND SM01.得意先部課ＣＤ = SM04.得意先部課ＣＤ ");
// MOD 2011.03.09 東都）高木 請求先マスタの主キーに[会員ＣＤ]を追加 START
				sbQuery.Append("AND SM01.会員ＣＤ = SM04.会員ＣＤ ");
// MOD 2011.03.09 東都）高木 請求先マスタの主キーに[会員ＣＤ]を追加 END
				sbQuery.Append("AND SM04.削除ＦＧ = '0' \n");
				sbQuery.Append("WHERE SM01.会員ＣＤ = '" + s会員ＣＤ + "' \n");
				sbQuery.Append(  "AND SM01.部門ＣＤ = '" + s部門ＣＤ + "' \n");
				sbQuery.Append(  "AND SM01.荷送人ＣＤ = '" + s依頼主ＣＤ + "' \n");
				sbQuery.Append(  "AND SM01.削除ＦＧ = '0' \n");

				// MOD-S 2012.09.06 COA)横山 Oracleサーバ負荷軽減対策（SQLにバインド変数を利用）
				//OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);
				logWriter(sUser, INF_SQL, "###バインド後（想定）###\n" + sbQuery.ToString());	//修正前のUPDATE文をログ出力

				sbQuery = new StringBuilder(1024);
				sbQuery.Append("SELECT SM01.カナ略称 \n");
				sbQuery.Append(      ",SM01.電話番号１ \n");
				sbQuery.Append(      ",SM01.電話番号２ \n");
				sbQuery.Append(      ",SM01.電話番号３ \n");
				sbQuery.Append(      ",SM01.郵便番号 \n");
				sbQuery.Append(      ",SM01.住所１ \n");
				sbQuery.Append(      ",SM01.住所２ \n");
				sbQuery.Append(      ",SM01.名前１ \n");
				sbQuery.Append(      ",SM01.名前２ \n");
				sbQuery.Append(      ",SM01.才数 \n");
				sbQuery.Append(      ",SM01.重量 \n");
				sbQuery.Append(      ",SM01.\"メールアドレス\" \n");
				sbQuery.Append(      ",SM01.得意先ＣＤ \n");
				sbQuery.Append(      ",SM01.得意先部課ＣＤ \n");
				sbQuery.Append(      ",SM01.更新日時 \n");
				sbQuery.Append(      ",NVL(SM04.得意先部課名,' ') \n");
				sbQuery.Append( "FROM ＳＭ０１荷送人 SM01 \n");
				sbQuery.Append(" LEFT JOIN \"ＣＭ０２部門\" CM02 \n");
				sbQuery.Append(" ON SM01.会員ＣＤ = CM02.会員ＣＤ ");
				sbQuery.Append("AND SM01.部門ＣＤ = CM02.部門ＣＤ ");
				sbQuery.Append("AND CM02.削除ＦＧ = '0' \n");
				sbQuery.Append(" LEFT JOIN \"ＳＭ０４請求先\" SM04 \n");
				sbQuery.Append(" ON CM02.郵便番号 = SM04.郵便番号 ");
				sbQuery.Append("AND SM01.得意先ＣＤ = SM04.得意先ＣＤ ");
				sbQuery.Append("AND SM01.得意先部課ＣＤ = SM04.得意先部課ＣＤ ");
				sbQuery.Append("AND SM01.会員ＣＤ = SM04.会員ＣＤ ");
				sbQuery.Append("AND SM04.削除ＦＧ = '0' \n");
				sbQuery.Append("WHERE SM01.会員ＣＤ   = :p_KaiinCD \n");
				sbQuery.Append(  "AND SM01.部門ＣＤ   = :p_BumonCD \n");
				sbQuery.Append(  "AND SM01.荷送人ＣＤ = :p_NisouCD \n");
				sbQuery.Append(  "AND SM01.削除ＦＧ = '0' \n");

				wk_opOraParam = new OracleParameter[3];
				wk_opOraParam[0] = new OracleParameter("p_KaiinCD", OracleDbType.Char, s会員ＣＤ,   ParameterDirection.Input);
				wk_opOraParam[1] = new OracleParameter("p_BumonCD", OracleDbType.Char, s部門ＣＤ,   ParameterDirection.Input);
				wk_opOraParam[2] = new OracleParameter("p_NisouCD", OracleDbType.Char, s依頼主ＣＤ, ParameterDirection.Input);

				OracleDataReader	reader = CmdSelect(sUser, conn2, sbQuery, wk_opOraParam);
				wk_opOraParam = null;
				// MOD-E 2012.09.06 COA)横山 Oracleサーバ負荷軽減対策（SQLにバインド変数を利用）
// MOD 2010.09.27 東都）高木 請求先部課名の追加 END

				bool bRead = reader.Read();
				if(bRead == true)
				{
// MOD 2005.05.11 東都）高木 ORA-03113対策？ START
//					for(int iCnt = 1; iCnt < 17; iCnt++)
//					{
//						sRet[iCnt] = reader.GetString(iCnt - 1).Trim();
//					}
// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない START
//					sRet[1]  = reader.GetString(0).Trim();
					sRet[1]  = reader.GetString(0).TrimEnd();
// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない END
					sRet[2]  = reader.GetString(1).Trim();
					sRet[3]  = reader.GetString(2).Trim();
					sRet[4]  = reader.GetString(3).Trim();
					sRet[5]  = reader.GetString(4).Trim();	// 郵便番号
					sRet[6]  = "";
					if(sRet[5].Length > 3)
					{
						sRet[6]  = sRet[5].Substring(3);
// MOD 2005.09.02 東都）小童谷 Trim追加 START
//						sRet[5]  = sRet[5].Substring(0,3);
						sRet[5]  = sRet[5].Substring(0,3).Trim();
// MOD 2005.09.02 東都）小童谷 Trim追加 END
					}
// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない START
//					sRet[7]  = reader.GetString(5).Trim();
//					sRet[8]  = reader.GetString(6).Trim();
//					sRet[9]  = reader.GetString(7).Trim();
//					sRet[10] = reader.GetString(8).Trim();
					sRet[7]  = reader.GetString(5).TrimEnd(); // 住所１
					sRet[8]  = reader.GetString(6).TrimEnd(); // 住所２
					sRet[9]  = reader.GetString(7).TrimEnd(); // 名前１
					sRet[10] = reader.GetString(8).TrimEnd(); // 名前２
// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない END
					sRet[11] = reader.GetDecimal(9).ToString().Trim();	// 才数
					sRet[12] = reader.GetDecimal(10).ToString().Trim();	// 重量
					sRet[13] = reader.GetString(11).Trim();
					sRet[14] = reader.GetString(12).Trim();
					sRet[15] = reader.GetString(13).Trim();
					sRet[16] = reader.GetDecimal(14).ToString().Trim();	// 更新日時

					sRet[0] = "更新";
					sRet[17] = "U";
// MOD 2010.09.27 東都）高木 請求先部課名の追加 START
					sRet[18] = reader.GetString(15).TrimEnd(); // 得意先部課名
// MOD 2010.09.27 東都）高木 請求先部課名の追加 END
				}
				else
				{
					sRet[0] = "登録";
					sRet[17] = "I";
				}
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END

				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				conn2 = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
			}
			
			return sRet;
		}

		/*********************************************************************
		 * 依頼主データ更新
		 * 引数：会員ＣＤ、部門ＣＤ、荷送人ＣＤ...
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Upd_irainusi(string[] sUser, string[] sData)
		{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//			logFileOpen(sUser);
			logWriter(sUser, INF, "依頼主更新開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[5];
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//// ADD 2005.05.23 東都）小童谷 会員チェック追加 START
//			// 会員チェック
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 東都）小童谷 会員チェック追加 END

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				string cmdQuery 
					= "UPDATE ＳＭ０１荷送人 \n"
					+    "SET カナ略称           = '" + sData[1]  +"', \n"
					+        "電話番号１         = '" + sData[2]  +"', \n"
					+        "電話番号２         = '" + sData[3]  +"', \n"
					+        "電話番号３         = '" + sData[4]  +"', \n"
					+        "郵便番号           = '" + sData[5]  + sData[6] + "', \n"
					+        "住所１             = '" + sData[7]  +"', \n"
					+        "住所２             = '" + sData[8]  +"', \n"
					+        "名前１             = '" + sData[9]  +"', \n"
					+        "名前２             = '" + sData[10] +"', \n"
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
					;
				string s重量入力制御 = (sData.Length > 23) ? sData[23] : "0";
				if(s重量入力制御 == "1"){
					cmdQuery = cmdQuery
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
// MOD 2005.05.17 東都）小童谷 才数復活 START
					+        "才数               =  " + sData[11] +", \n"
// MOD 2005.05.17 東都）小童谷 才数復活 END
					+        "重量               =  " + sData[12] +", \n"
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
					;
				}
				cmdQuery = cmdQuery
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
					+        "\"メールアドレス\" = '" + sData[13] +"', \n"
					+        "得意先ＣＤ         = '" + sData[19] +"', \n"
					+        "得意先部課ＣＤ     = '" + sData[20] +"', \n"
					+        "削除ＦＧ           = '0', \n"
					+        "更新ＰＧ           = '" + sData[14] +"', \n"
					+        "更新者             = '" + sData[15] +"', \n"
					+        "更新日時           =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
					+ " WHERE 会員ＣＤ           = '" + sData[16] +"' \n"
					+ "   AND 部門ＣＤ           = '" + sData[17] +"' \n"
					+ "   AND 荷送人ＣＤ         = '" + sData[0] +"' \n"
					+ "   AND 更新日時           =  " + sData[18] +"";

				int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);

				if(iUpdRow == 0)
					sRet[0] = "データ編集中に他の端末より更新されています。\r\n再度、最新データを呼び出して更新してください。";
				else
				{
					sRet[0] = "正常終了";
					if(sData[21] == "1")
					{
						bool riyou = Upd_riyou(sUser, conn2, sData[16], sData[15], sData[0], sData[14]);
						if(riyou == false)
							sRet[0] = "デフォルトに設定できませんでした";
					}
				}
				tran.Commit();

				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				conn2 = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
			}
			
			return sRet;
		}

		/*********************************************************************
		 * 依頼主データ登録
		 * 引数：会員ＣＤ、部門ＣＤ、荷送人ＣＤ...
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Ins_irainusi(string[] sUser, string[] sData)
		{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//			logFileOpen(sUser);
			logWriter(sUser, INF, "依頼主登録開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[5];
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//// ADD 2005.05.23 東都）小童谷 会員チェック追加 START
//			// 会員チェック
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 東都）小童谷 会員チェック追加 END

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
// MOD 2010.09.17 東都）高木 複数件削除機能の追加 START
//				string cmdQuery 
//					= "DELETE FROM ＳＭ０１荷送人 "
//					+ " WHERE 会員ＣＤ           = '" + sData[16] +"'"
//					+ "   AND 部門ＣＤ           = '" + sData[17] +"'"
//					+ "   AND 荷送人ＣＤ         = '" + sData[0] +"'"
//					+ "   AND 削除ＦＧ           = '1'";
//
//				CmdUpdate(sUser, conn2, cmdQuery);
				string cmdQuery = "";
				cmdQuery
					= "SELECT 削除ＦＧ \n"
					+   "FROM ＳＭ０１荷送人 \n"
					+  "WHERE 会員ＣＤ = '" + sData[16] + "' \n"
					+    "AND 部門ＣＤ = '" + sData[17] + "' \n"
					+    "AND 荷送人ＣＤ = '" + sData[0] + "' "
					+    "FOR UPDATE "
					;

				OracleDataReader reader;
				reader = CmdSelect(sUser, conn2, cmdQuery);
				string s削除ＦＧ = "";
				if(reader.Read()){
					s削除ＦＧ = reader.GetString(0);
				}
				reader.Close();
				disposeReader(reader);
				reader = null;

// MOD 2011.04.13 東都）高木 重量入力不可対応 START
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
				string s重量入力制御 = (sData.Length > 23) ? sData[23] : "0";
				if(s重量入力制御 != "1"){
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
					sData[11] = "0"; // 才数
					sData[12] = "0"; // 重量
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
				}
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
// MOD 2011.04.13 東都）高木 重量入力不可対応 END
				if(s削除ＦＧ == "0"){
					sRet[0] = "データ編集中に他の端末より更新されています。\r\n"
							+ "再度、最新データを呼び出して更新してください。";
					tran.Commit();
					logWriter(sUser, INF, sRet[0]);
					return sRet;
				}else if(s削除ＦＧ == "1"){
					cmdQuery 
						= "UPDATE ＳＭ０１荷送人 \n"
						+    "SET "
						+        "得意先ＣＤ         = '" + sData[19] +"', \n"
						+        "得意先部課ＣＤ     = '" + sData[20] +"', \n"
						+        "電話番号１         = '" + sData[2]  +"', \n"
						+        "電話番号２         = '" + sData[3]  +"', \n"
						+        "電話番号３         = '" + sData[4]  +"', \n"
						+        "ＦＡＸ番号１       = ' ', \n"
						+        "ＦＡＸ番号２       = ' ', \n"
						+        "ＦＡＸ番号３       = ' ', \n"
						+        "住所１             = '" + sData[7]  +"', \n"
						+        "住所２             = '" + sData[8]  +"', \n"
						+        "住所３             = ' ', \n"
						+        "名前１             = '" + sData[9]  +"', \n"
						+        "名前２             = '" + sData[10] +"', \n"
						+        "名前３             = ' ', \n"
						+        "郵便番号           = '" + sData[5]  + sData[6] + "', \n"
						+        "カナ略称           = '" + sData[1]  +"', \n"
						+        "才数               =  " + sData[11] +", \n"
						+        "重量               =  " + sData[12] +", \n"
						+        "荷札区分           = ' ', \n"
						+        "\"メールアドレス\" = '" + sData[13] +"', \n"
						+        "削除ＦＧ           = '0', \n"
						+        "登録日時           =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), \n"
						+        "登録ＰＧ           = '" + sData[14] +"', \n"
						+        "登録者             = '" + sData[15] +"', \n"
						+        "更新日時           =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), \n"
						+        "更新ＰＧ           = '" + sData[14] +"', \n"
						+        "更新者             = '" + sData[15] +"'  \n"
						+ " WHERE 会員ＣＤ           = '" + sData[16] +"' \n"
						+ "   AND 部門ＣＤ           = '" + sData[17] +"' \n"
						+ "   AND 荷送人ＣＤ         = '" + sData[0] +"' \n"
						;
				}else{
// MOD 2010.09.17 東都）高木 複数件削除機能の追加 END
					cmdQuery 
						= "INSERT INTO ＳＭ０１荷送人 "
						+ "VALUES ('" + sData[16] +"','" + sData[17] +"','" + sData[0] +"', \n"
						+         "'" + sData[19] +"','" + sData[20] +"','" + sData[2] +"','" + sData[3] +"', \n"
						+         "'" + sData[4] +"',' ',' ',' ','" + sData[7] +"','" + sData[8] +"', \n"
						+         "' ','" + sData[9] +"','" + sData[10] +"',' ','" + sData[5] + sData[6] +"', \n"
						+         "'" + sData[1] +"'," + sData[11] +"," + sData[12] +",' ','" + sData[13] +"', \n"
						+         "'0',TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'),'" + sData[14] +"','" + sData[15] +"', \n"
						+         "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'),'" + sData[14] +"','" + sData[15] +"')";
// MOD 2010.09.17 東都）高木 複数件削除機能の追加 START
				}
// MOD 2010.09.17 東都）高木 複数件削除機能の追加 END

				CmdUpdate(sUser, conn2, cmdQuery);

				sRet[0] = "正常終了";

				if(sData[21] == "1")
				{
					bool riyou = Upd_riyou(sUser, conn2, sData[16], sData[15], sData[0], sData[14]);
					if(riyou == false)
						sRet[0] = "デフォルトに設定できませんでした";
				}
				tran.Commit();
				
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				conn2 = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
			}
			
			return sRet;
		}

		/*********************************************************************
		 * 依頼主データ削除
		 * 引数：会員ＣＤ、部門ＣＤ、荷送人ＣＤ、更新ＰＧ、更新者
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Del_irainusi(string[] sUser, string[] sData)
		{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//			logFileOpen(sUser);
			logWriter(sUser, INF, "依頼主削除開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[5];
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//// ADD 2005.05.23 東都）小童谷 会員チェック追加 START
//			// 会員チェック
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 東都）小童谷 会員チェック追加 END

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				string cmdQuery 
					= "UPDATE ＳＭ０１荷送人 "
					+    "SET 削除ＦＧ           = '1', \n"
					+        "更新ＰＧ           = '" + sData[3] +"', \n"
					+        "更新者             = '" + sData[4] +"', \n"
					+        "更新日時           =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
					+ " WHERE 会員ＣＤ           = '" + sData[0] +"' \n"
					+ "   AND 部門ＣＤ           = '" + sData[1] +"' \n"
					+ "   AND 荷送人ＣＤ         = '" + sData[2] +"'";

				CmdUpdate(sUser, conn2, cmdQuery);

				tran.Commit();				
				sRet[0] = "正常終了";
				
				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				conn2 = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
			}
			
			return sRet;
		}

		/*********************************************************************
		 * 利用者データ更新
		 * 引数：会員ＣＤ、利用者ＣＤ、荷送人ＣＤ、更新ＰＧ、更新者
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		private bool Upd_riyou(string[] sUser, OracleConnection conn2, string sKcode, string sRcode, string sIcode, string sPg)
		{
			string cmdQuery
				= "UPDATE ＣＭ０４利用者 "
				+    "SET 荷送人ＣＤ     = '" + sIcode + "', \n"
				+        "更新日時       = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), \n"
				+        "更新ＰＧ       = '" + sPg    + "', \n"
				+        "更新者         = '" + sRcode + "'  \n"
				+  "WHERE 会員ＣＤ       = '" + sKcode + "'  \n"
				+    "AND 利用者ＣＤ     = '" + sRcode + "'  ";

			int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);

			if(iUpdRow == 0)
				return false;
			else
				return true;
		}

// ADD 2005.11.07 東都）伊賀 出荷ジャーナルチェック追加 START
		/*********************************************************************
		 * 依頼主使用中チェック
		 * 引数：会員ＣＤ、利用者ＣＤ、荷送人ＣＤ
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Sel_SyukkaIrainusi(string[] sUser, string[] sData)
		{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//			logFileOpen(sUser);
			logWriter(sUser, INF, "依頼主使用中チェック開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[2];
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//			// 会員チェック
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}

			try
			{
				string cmdQuery 
					= "SELECT COUNT(*) \n" 
					+   "FROM \"ＳＴ０１出荷ジャーナル\" \n"
					+ " WHERE 会員ＣＤ           = '" + sData[0] +"' \n"
					+ "   AND 部門ＣＤ           = '" + sData[1] +"' \n"
					+ "   AND 荷送人ＣＤ         = '" + sData[2] +"'"
					+ "   AND 削除ＦＧ           = '0' \n";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

				reader.Read();
				if (reader.GetDecimal(0) == 0)
				{
					sRet[0] = "正常終了";
					sRet[1] = "0";
				}
				else
				{
					sRet[0] = "出荷データが存在するため削除できません";
					sRet[1] = reader.GetDecimal(0).ToString().Trim();
				}
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END

				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				conn2 = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
			}
			
			return sRet;
		}
// ADD 2005.11.07 東都）伊賀 出荷ジャーナルチェック追加 END

// ADD 2006.08.10 東都）山本 利用者コード取得追加 START

		/*********************************************************************
		 * 依頼主データ更新
		 * 引数：会員ＣＤ、部門ＣＤ、荷送人ＣＤ...
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Upd_irainusi1(string[] sUser, string[] sData)
		{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//			logFileOpen(sUser);
			logWriter(sUser, INF, "依頼主更新開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[5];
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//// ADD 2005.05.23 東都）小童谷 会員チェック追加 START
//			// 会員チェック
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 東都）小童谷 会員チェック追加 END

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				string cmdQuery 
					= "UPDATE ＳＭ０１荷送人 \n"
					+    "SET カナ略称           = '" + sData[1]  +"', \n"
					+        "電話番号１         = '" + sData[2]  +"', \n"
					+        "電話番号２         = '" + sData[3]  +"', \n"
					+        "電話番号３         = '" + sData[4]  +"', \n"
					+        "郵便番号           = '" + sData[5]  + sData[6] + "', \n"
					+        "住所１             = '" + sData[7]  +"', \n"
					+        "住所２             = '" + sData[8]  +"', \n"
					+        "名前１             = '" + sData[9]  +"', \n"
					+        "名前２             = '" + sData[10] +"', \n"
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
					;
				string s重量入力制御 = (sData.Length > 23) ? sData[23] : "0";
				if(s重量入力制御 == "1"){
					cmdQuery = cmdQuery
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
// MOD 2005.05.17 東都）小童谷 才数復活 START
					+        "才数               =  " + sData[11] +", \n"
// MOD 2005.05.17 東都）小童谷 才数復活 END
					+        "重量               =  " + sData[12] +", \n"
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
					;
				}
				cmdQuery = cmdQuery
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
					+        "\"メールアドレス\" = '" + sData[13] +"', \n"
					+        "得意先ＣＤ         = '" + sData[19] +"', \n"
					+        "得意先部課ＣＤ     = '" + sData[20] +"', \n"
					+        "削除ＦＧ           = '0', \n"
					+        "更新ＰＧ           = '" + sData[14] +"', \n"
					+        "更新者             = '" + sData[15] +"', \n"
					+        "更新日時           =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
					+ " WHERE 会員ＣＤ           = '" + sData[16] +"' \n"
					+ "   AND 部門ＣＤ           = '" + sData[17] +"' \n"
					+ "   AND 荷送人ＣＤ         = '" + sData[0] +"' \n"
					+ "   AND 更新日時           =  " + sData[18] +"";

				int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);

				if(iUpdRow == 0)
					sRet[0] = "データ編集中に他の端末より更新されています。\r\n再度、最新データを呼び出して更新してください。";
				else
				{
					sRet[0] = "正常終了";
					if(sData[22] == "1")
					{
						bool riyou = Upd_riyou(sUser, conn2, sData[16], sData[15], "            ", sData[14]);
						if(riyou == false)
							sRet[0] = "デフォルトに設定できませんでした";
					}
					else
					{
						if(sData[21] == "1")
						{
							bool riyou = Upd_riyou(sUser, conn2, sData[16], sData[15], sData[0], sData[14]);
							if(riyou == false)
								sRet[0] = "デフォルトに設定できませんでした";
						}
					}
				}
				tran.Commit();

				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				conn2 = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
			}
			
			return sRet;
		}


		/*********************************************************************
		 * 利用者コード取得
		 * 引数：会員ＣＤ、利用者ＣＤ
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Get_riyo(string[] sUser, string sKcode, string sRcode)
		{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//			logFileOpen(sUser);
			logWriter(sUser, INF, "利用者コード取得開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[18];

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//			// 会員チェック
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}

			try
			{
				string cmdQuery
					= "SELECT 荷送人ＣＤ \n"
					+    "FROM ＣＭ０４利用者 \n"
					+  "WHERE 会員ＣＤ       = '" + sKcode + "'  \n"
					+    "AND 利用者ＣＤ     = '" + sRcode + "'  ";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

				bool bRead = reader.Read();
				if(bRead == true)
				{
					sRet[0]  = reader.GetString(0).Trim();
				}
				else
				{
					sRet[0] = "該当データ無し";
				}
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END

				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				conn2 = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
			}
			
			return sRet;
		}
// ADD 2006.08.10 東都）山本 利用者コード取得追加 END
// MOD 2010.09.08 東都）高木 ＣＳＶ出力機能の追加 START
		/*********************************************************************
		 * ＣＳＶ出力
		 * 引数：会員ＣＤ、利用者ＣＤ
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Get_csvwrite(string[] sUser, string sKCode, string sBCode)
		{
			logWriter(sUser, INF, "ＣＳＶ出力開始");
			string[] sKey = new string[]{sKCode, sBCode};
			return Get_csvwrite2(sUser, sKey);
		}
		/*********************************************************************
		 * ＣＳＶ出力２
		 * 引数：会員ＣＤ、利用者ＣＤ
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Get_csvwrite2(string[] sUser, string[] sKey)
		{
			string s会員ＣＤ     = (sKey.Length >  0) ? sKey[ 0] : "";
			string s部門ＣＤ     = (sKey.Length >  1) ? sKey[ 1] : "";
			string s依頼主カナ   = (sKey.Length >  2) ? sKey[ 2] : "";
			string s依頼主コード = (sKey.Length >  3) ? sKey[ 3] : "";
			string s依頼主名前   = (sKey.Length >  4) ? sKey[ 4] : "";
			string s請求先ＣＤ   = (sKey.Length >  5) ? sKey[ 5] : "";
			string s部課ＣＤ     = (sKey.Length >  6) ? sKey[ 6] : "";
			string s階層リスト１ = (sKey.Length >  7) ? sKey[ 7] : "2";
			string sソート方向１ = (sKey.Length >  8) ? sKey[ 8] : "0";
			string s階層リスト２ = (sKey.Length >  9) ? sKey[ 9] : "0";
			string sソート方向２ = (sKey.Length > 10) ? sKey[10] : "0";

			int i階層リスト１ = 2;
			int iソート方向１ = 0;
			int i階層リスト２ = 0;
			int iソート方向２ = 0;
			try{
				i階層リスト１ = int.Parse(s階層リスト１);
				iソート方向１ = int.Parse(sソート方向１);
				i階層リスト２ = int.Parse(s階層リスト２);
				iソート方向２ = int.Parse(sソート方向２);
			}catch(Exception){
				;
			}

			if(sKey.Length >  2){
				logWriter(sUser, INF, "ＣＳＶ出力２開始");
			}
			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();

			string[] sRet = new string[1];
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			StringBuilder sbQuery = new StringBuilder(1024);
			try
			{
				sbQuery.Append("SELECT SM01.荷送人ＣＤ \n");
				sbQuery.Append(      ",SM01.電話番号１ \n");
				sbQuery.Append(      ",SM01.電話番号２ \n");
				sbQuery.Append(      ",SM01.電話番号３ \n");
				sbQuery.Append(      ",SM01.住所１ \n");
				sbQuery.Append(      ",SM01.住所２ \n");
				sbQuery.Append(      ",SM01.住所３ \n");
				sbQuery.Append(      ",SM01.名前１ \n");
				sbQuery.Append(      ",SM01.名前２ \n");
				sbQuery.Append(      ",SM01.名前３ \n");
				sbQuery.Append(      ",SM01.郵便番号 \n");
				sbQuery.Append(      ",SM01.カナ略称 \n");
				sbQuery.Append(      ",SM01.才数 \n");
				sbQuery.Append(      ",SM01.重量 \n");
				sbQuery.Append(      ",SM01.\"メールアドレス\" \n");
				sbQuery.Append(      ",SM01.得意先ＣＤ \n");
				sbQuery.Append(      ",SM01.得意先部課ＣＤ \n");
				sbQuery.Append(      ",NVL(SM04.得意先部課名,' ') \n");
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
				sbQuery.Append(      ",NVL(CM01.保留印刷ＦＧ,'0') \n");
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
				sbQuery.Append( "FROM ＳＭ０１荷送人 SM01 \n");
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
				sbQuery.Append(" LEFT JOIN ＣＭ０１会員 CM01 \n");
				sbQuery.Append(" ON SM01.会員ＣＤ = CM01.会員ＣＤ \n");
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
				sbQuery.Append(" LEFT JOIN \"ＣＭ０２部門\" CM02 \n");
				sbQuery.Append(" ON SM01.会員ＣＤ = CM02.会員ＣＤ ");
				sbQuery.Append("AND SM01.部門ＣＤ = CM02.部門ＣＤ ");
				sbQuery.Append("AND CM02.削除ＦＧ = '0' \n");
				sbQuery.Append(" LEFT JOIN \"ＳＭ０４請求先\" SM04 \n");
				sbQuery.Append(" ON CM02.郵便番号 = SM04.郵便番号 ");
				sbQuery.Append("AND SM01.得意先ＣＤ = SM04.得意先ＣＤ ");
				sbQuery.Append("AND SM01.得意先部課ＣＤ = SM04.得意先部課ＣＤ ");
// MOD 2011.03.09 東都）高木 請求先マスタの主キーに[会員ＣＤ]を追加 START
				sbQuery.Append("AND SM01.会員ＣＤ = SM04.会員ＣＤ ");
// MOD 2011.03.09 東都）高木 請求先マスタの主キーに[会員ＣＤ]を追加 END
				sbQuery.Append("AND SM04.削除ＦＧ = '0' \n");
				sbQuery.Append("WHERE SM01.会員ＣＤ = '" + s会員ＣＤ + "' \n");
				sbQuery.Append(  "AND SM01.部門ＣＤ = '" + s部門ＣＤ + "' \n");

				if(s依頼主カナ.Length > 0){
					sbQuery.Append(" AND SM01.カナ略称 LIKE '"+ s依頼主カナ + "%' \n");
				}
				if(s依頼主コード.Length > 0){
					sbQuery.Append(" AND SM01.荷送人ＣＤ LIKE '"+ s依頼主コード + "%' \n");
				}
				if(s依頼主名前.Length > 0){
					sbQuery.Append(" AND SM01.名前１ LIKE '%"+ s依頼主名前 + "%' \n");
				}
				if(s請求先ＣＤ.Length > 0){
					sbQuery.Append(" AND SM01.得意先ＣＤ = '"+ s請求先ＣＤ + "' \n");
					if(s部課ＣＤ.Length > 0){
						sbQuery.Append(" AND SM01.得意先部課ＣＤ = '"+ s部課ＣＤ + "' \n");
					}else{
						sbQuery.Append(" AND SM01.得意先部課ＣＤ = ' ' \n");
					}
				}
				sbQuery.Append(  "AND SM01.削除ＦＧ = '0' \n");
				sbQuery.Append(  "ORDER BY \n");

				switch(i階層リスト１){
				case 1:
					sbQuery.Append(" SM01.カナ略称 ");
					if(iソート方向１ == 1) sbQuery.Append(" DESC ");
					break;
				case 2:
					sbQuery.Append(" SM01.荷送人ＣＤ");
					if(iソート方向１ == 1) sbQuery.Append(" DESC ");
					break;
				case 3:
//					sbQuery.Append(" SM01.得意先ＣＤ ");
//					if(iソート方向１ == 1) sbQuery.Append(" DESC ");
//					sbQuery.Append(", SM01.得意先部課ＣＤ ");
					sbQuery.Append(" NVL(SM04.得意先部課名,SM01.得意先ＣＤ || SM01.得意先部課ＣＤ) ");
					if(iソート方向１ == 1) sbQuery.Append(" DESC ");
					break;
				case 4:
					sbQuery.Append(" SM01.名前１ ");
					if(iソート方向１ == 1) sbQuery.Append(" DESC ");
//					sbQuery.Append(", SM01.名前２ ");
//					if(iソート方向１ == 1) sbQuery.Append(" DESC ");
					break;
				case 5:
					sbQuery.Append(" SM01.電話番号１ ");
					if(iソート方向１ == 1) sbQuery.Append(" DESC ");
					sbQuery.Append(", SM01.電話番号２ ");
					if(iソート方向１ == 1) sbQuery.Append(" DESC ");
					sbQuery.Append(", SM01.電話番号３ ");
					if(iソート方向１ == 1) sbQuery.Append(" DESC ");
					break;
				case 6:
					sbQuery.Append(" SM01.登録日時 ");
					if(iソート方向１ == 1) sbQuery.Append(" DESC ");
					break;
				case 7:
					sbQuery.Append(" SM01.更新日時 ");
					if(iソート方向１ == 1) sbQuery.Append(" DESC ");
					break;
				}
				if(i階層リスト１ != 0 && i階層リスト２ != 0){
					sbQuery.Append(",");
				}
				switch(i階層リスト２){
				case 1:
					sbQuery.Append(" SM01.カナ略称 ");
					if(iソート方向２ == 1) sbQuery.Append(" DESC ");
					break;
				case 2:
					sbQuery.Append(" SM01.荷送人ＣＤ");
					if(iソート方向２ == 1) sbQuery.Append(" DESC ");
					break;
				case 3:
//					sbQuery.Append(" SM01.得意先ＣＤ ");
//					if(iソート方向２ == 1) sbQuery.Append(" DESC ");
//					sbQuery.Append(", SM01.得意先部課ＣＤ ");
					sbQuery.Append(" NVL(SM04.得意先部課名,SM01.得意先ＣＤ || SM01.得意先部課ＣＤ) ");
					if(iソート方向２ == 1) sbQuery.Append(" DESC ");
					break;
				case 4:
					sbQuery.Append(" SM01.名前１ ");
					if(iソート方向２ == 1) sbQuery.Append(" DESC ");
//					sbQuery.Append(", SM01.名前２ ");
//					if(iソート方向２ == 1) sbQuery.Append(" DESC ");
					break;
				case 5:
					sbQuery.Append(" SM01.電話番号１ ");
					if(iソート方向２ == 1) sbQuery.Append(" DESC ");
					sbQuery.Append(", SM01.電話番号２ ");
					if(iソート方向２ == 1) sbQuery.Append(" DESC ");
					sbQuery.Append(", SM01.電話番号３ ");
					if(iソート方向２ == 1) sbQuery.Append(" DESC ");
					break;
				case 6:
					sbQuery.Append(" SM01.登録日時 ");
					if(iソート方向２ == 1) sbQuery.Append(" DESC ");
					break;
				case 7:
					sbQuery.Append(" SM01.更新日時 ");
					if(iソート方向２ == 1) sbQuery.Append(" DESC ");
					break;
				}
				if(i階層リスト１ == 0 && i階層リスト２ == 0){
					sbQuery.Append(" SM01.名前１ \n");
				}
				if(i階層リスト１ != 2 && i階層リスト２ != 2){
					sbQuery.Append(", SM01.荷送人ＣＤ \n");
				}
// MOD 2009.11.30 東都）高木 検索条件に名前、請求先を追加 END

				OracleDataReader reader;
				reader = CmdSelect(sUser, conn2, sbQuery);

				StringBuilder sbData;
				while (reader.Read()){
					sbData = new StringBuilder(1024);
					sbData.Append(sDbl + sSng + reader.GetString(0).TrimEnd() + sDbl);	// 荷送人ＣＤ
					sbData.Append(sKanma + sDbl + sSng);
					if(reader.GetString(1).TrimEnd().Length > 0){
						sbData.Append("(" + reader.GetString(1).TrimEnd() + ")");		// 電話番号１
					}
					if(reader.GetString(2).TrimEnd().Length > 0){
						sbData.Append(reader.GetString(2).TrimEnd() + "-");				// 電話番号２
					}
					sbData.Append(reader.GetString(3).TrimEnd() + sDbl);				// 電話番号３
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(4).TrimEnd() + sDbl);	// 住所１
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(5).TrimEnd() + sDbl);	// 住所２
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(6).TrimEnd() + sDbl);	// 住所３
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(7).TrimEnd() + sDbl);// 名前１
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(8).TrimEnd() + sDbl);// 名前２
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(9).TrimEnd() + sDbl);// 名前３
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(10).TrimEnd() + sDbl);// 郵便番号
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(11).TrimEnd() + sDbl);// カナ略称
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
					string s重量入力制御 = reader.GetString(18).TrimEnd();
					if(s重量入力制御 == "1"){
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
						sbData.Append(sKanma + sDbl + sSng + reader.GetDecimal(12).ToString().TrimEnd() + sDbl);// 才数
						sbData.Append(sKanma + sDbl + sSng + reader.GetDecimal(13).ToString().TrimEnd() + sDbl);// 重量
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
					}else{
						sbData.Append(sKanma + sDbl + sSng + "0" + sDbl);// 才数
						sbData.Append(sKanma + sDbl + sSng + "0" + sDbl);// 重量
					}
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(14).TrimEnd() + sDbl);// メールアドレス
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(15).TrimEnd() + sDbl);// 得意先ＣＤ
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(16).TrimEnd() + sDbl);// 得意先部課ＣＤ
					sList.Add(sbData);
				}
				disposeReader(reader);
				reader = null;

				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "該当データがありません";
				else
				{
					sRet[0] = "正常終了";
					int iCnt = 1;
					IEnumerator enumList = sList.GetEnumerator();
					while(enumList.MoveNext())
					{
						sRet[iCnt] = enumList.Current.ToString();
						iCnt++;
					}
				}
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}
// MOD 2010.09.08 東都）高木 ＣＳＶ出力機能の追加 END
// MOD 2010.09.08 東都）高木 ＣＳＶ取込機能の追加 START
		/*********************************************************************
		 * アップロードデータ追加２
		 * 引数：会員ＣＤ、部門ＣＤ、荷受人ＣＤ...
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		private static string INS_UPLOADDATA2_SELECT1
			= "SELECT 1 \n"
			+ " FROM ＣＭ１４郵便番号 \n"
			;

		private static string INS_UPLOADDATA2_SELECT2
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
//			= "SELECT 郵便番号 \n"
//			+ " FROM ＣＭ０２部門 \n"
			= "SELECT CM02.郵便番号 \n"
			+ ", NVL(CM01.保留印刷ＦＧ,'0') \n"
			+ " FROM ＣＭ０２部門 CM02 \n"
			+ " LEFT JOIN ＣＭ０１会員 CM01 \n"
			+ " ON CM02.会員ＣＤ = CM01.会員ＣＤ \n"
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
			;

		private static string INS_UPLOADDATA2_SELECT3
			= "SELECT 1 \n"
			+ " FROM ＳＭ０４請求先 \n"
			;

		[WebMethod]
		public String[] Ins_uploadData2(string[] sUser, string[] sList)
		{
			logWriter(sUser, INF, "アップロードデータ追加２開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[(sList.Length*2) + 1];

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null){
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();
			OracleDataReader reader;
			string cmdQuery = "";

			sRet[0] = "";
			try{
				string s部郵便番号 = "";
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
				string s重量入力制御 = "0";
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
				for (int iRow = 0; iRow < sList.Length; iRow++){
					sRet[iRow*2+1] = "";
					sRet[iRow*2+2] = "";

					string[] sData = sList[iRow].Split(',');
					if(sData.Length != 21){
						throw new Exception("パラメータ長エラー["+sData.Length+"]");
					}

					string s会員ＣＤ   = sData[0];
					string s部門ＣＤ   = sData[1];
					string s荷送人ＣＤ = sData[2];
					string s郵便番号   = sData[12];
					string s請求先ＣＤ = sData[17];
					string s請求先部課 = sData[18];

					if(iRow == 0){
						//部門マスタの存在チェック
						cmdQuery = INS_UPLOADDATA2_SELECT2
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
//								+ "WHERE 会員ＣＤ = '" + s会員ＣＤ + "' \n"
//								+ "AND 部門ＣＤ = '" + s部門ＣＤ + "' \n"
//								+ "AND 削除ＦＧ = '0' \n"
								+ "WHERE CM02.会員ＣＤ = '" + s会員ＣＤ + "' \n"
								+ "AND CM02.部門ＣＤ = '" + s部門ＣＤ + "' \n"
								+ "AND CM02.削除ＦＧ = '0' \n"
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
								;

						reader = CmdSelect(sUser, conn2, cmdQuery);
						if(!reader.Read()){
							reader.Close();
							disposeReader(reader);
							reader = null;
							throw new Exception("セクションが存在しません");
						}
						s部郵便番号 = reader.GetString(0).TrimEnd();
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
						s重量入力制御 = reader.GetString(1).TrimEnd();
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
						reader.Close();
						disposeReader(reader);
						reader = null;
					}

					//郵便番号マスタの存在チェック
					cmdQuery = INS_UPLOADDATA2_SELECT1
// MOD 2010.09.29 東都）高木 郵便番号(__)対応（＊既存バグだが導入） START
//							+ "WHERE 郵便番号 = '" + s郵便番号 + "' \n"
//							+ "AND 削除ＦＧ = '0' \n"
							;
							string s郵便番号１ = "";
							string s郵便番号２ = "";
							if(s郵便番号.Length > 3){
								s郵便番号１ = s郵便番号.Substring(0,3).Trim();
								s郵便番号２ = s郵便番号.Substring(3).Trim();
								s郵便番号 = s郵便番号１ + s郵便番号２;
							}
							if(s郵便番号.Length == 7){
								cmdQuery += " WHERE 郵便番号 = '" + s郵便番号 + "' \n";
							}else{
								cmdQuery += " WHERE 郵便番号 LIKE '" + s郵便番号 + "%' \n";
							}
							cmdQuery += "AND 削除ＦＧ = '0' \n"
// MOD 2010.09.29 東都）高木 郵便番号(__)対応（＊既存バグだが導入） END
							;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					if(!reader.Read()){
						sRet[iRow*2+1] = s郵便番号.TrimEnd();//該当データ無し
//						reader.Close();
//						disposeReader(reader);
//						reader = null;
//						continue;
					}
					reader.Close();
					disposeReader(reader);
					reader = null;

					//請求先マスタの存在チェック
					cmdQuery = INS_UPLOADDATA2_SELECT3
							+ "WHERE 郵便番号 = '" + s部郵便番号 + "' \n"
							+ "AND 得意先ＣＤ = '" + s請求先ＣＤ + "' \n"
							+ "AND 得意先部課ＣＤ = '" + s請求先部課 + "' \n"
// MOD 2011.03.09 東都）高木 請求先マスタの主キーに[会員ＣＤ]を追加 START
							+ "AND 会員ＣＤ = '" + s会員ＣＤ + "' \n"
// MOD 2011.03.09 東都）高木 請求先マスタの主キーに[会員ＣＤ]を追加 END
 							+ "AND 削除ＦＧ = '0' \n"
							;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					if(!reader.Read()){
						sRet[iRow*2+2] = s請求先ＣＤ.TrimEnd(); //該当データ無し
						if(s請求先部課.TrimEnd().Length > 0){
							sRet[iRow*2+2] += "-" + s請求先部課.TrimEnd();
						}
//						reader.Close();
//						disposeReader(reader);
//						reader = null;
//						continue;
					}
					reader.Close();
					disposeReader(reader);
					reader = null;
					
					//エラーがあれば、次の行
					if(sRet[iRow*2+1].Length != 0 || sRet[iRow*2+2].Length != 0){
						continue;
					}

					cmdQuery
						= "SELECT 削除ＦＧ \n"
						+   "FROM ＳＭ０１荷送人 \n"
						+  "WHERE 会員ＣＤ = '" + s会員ＣＤ + "' \n"
						+    "AND 部門ＣＤ = '" + s部門ＣＤ + "' \n"
						+    "AND 荷送人ＣＤ = '" + s荷送人ＣＤ + "' "
						+    "FOR UPDATE "
						;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					int iCnt = 1;
					string s削除ＦＧ = "";
					while (reader.Read()){
						s削除ＦＧ = reader.GetString(0);
						iCnt++;
					}
					reader.Close();
					disposeReader(reader);
					reader = null;

					if(iCnt == 1){
						//追加
						cmdQuery 
							= "INSERT INTO ＳＭ０１荷送人 \n"
							+ "VALUES ( \n"
							+  "'" + sData[0] + "', "		//会員ＣＤ
							+  "'" + sData[1] + "', \n"		//部門ＣＤ
							+  "'" + sData[2] + "', \n"		//荷送人ＣＤ

							+  "'" + sData[17] + "', "		//得意先ＣＤ
							+  "'" + sData[18] + "', \n"	//得意先部課ＣＤ
							+  "'" + sData[3] + "', "		//電話番号
							+  "'" + sData[4] + "', "
							+  "'" + sData[5] + "', \n"
							+  "' ', "						//ＦＡＸ番号
							+  "' ', "
							+  "' ', \n"
							+  "'" + sData[6] + "', "		//住所
							+  "'" + sData[7] + "', "
							+  "'" + sData[8] + "', \n"
							+  "'" + sData[9] + "', "		//名前
							+  "'" + sData[10] + "', "
							+  "'" + sData[11] + "', \n"
							+  "'" + sData[12] + "', "		//郵便番号
							+  "'" + sData[13] + "', \n"	//カナ略称
							+  " " + sData[14] + " , "		//才数
							+  " " + sData[15] + " , \n"	//重量
							+  "' ', "						//荷札区分
							+  "'" + sData[16] + "', \n"	//メールアドレス
							+  "'0', "
							+  "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), "
							+  "'" + sData[19] + "', "
							+  "'" + sData[20] + "', \n"
							+  "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), "
							+  "'" + sData[19] + "', "
							+  "'" + sData[20] + "' \n"
							+  ") "
							;
						CmdUpdate(sUser, conn2, cmdQuery);
					}else{
						//上書き更新
						cmdQuery
							= "UPDATE ＳＭ０１荷送人 \n"
							+    "SET 得意先ＣＤ = '" + sData[17] + "' \n"
							+       ",得意先部課ＣＤ = '" + sData[18] + "' \n"
							+       ",電話番号１ = '" + sData[3] + "' \n"
							+       ",電話番号２ = '" + sData[4] + "' \n"
							+       ",電話番号３ = '" + sData[5] + "' \n"
							+       ",ＦＡＸ番号１ = ' ' \n"
							+       ",ＦＡＸ番号２ = ' ' \n"
							+       ",ＦＡＸ番号３ = ' ' \n"
							+       ",住所１ = '" + sData[6] + "' \n"
							+       ",住所２ = '" + sData[7] + "' \n"
							+       ",住所３ = '" + sData[8] + "' \n"
							+       ",名前１ = '" + sData[9] + "' \n"
							+       ",名前２ = '" + sData[10] + "' \n"
							+       ",名前３ = '" + sData[11] + "' \n"
							+       ",郵便番号 = '" + sData[12] + "' "
							+       ",カナ略称 = '" + sData[13] + "' "
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
							;
						if(s重量入力制御 == "1"){
							cmdQuery = cmdQuery
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
							+       ",才数 = "+ sData[14] +" "
							+       ",重量 = "+ sData[15] +" "
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
							;
						}
						cmdQuery = cmdQuery
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
							+       ",荷札区分 = ' ' "
							+       ",\"メールアドレス\" = '"+ sData[16] +"' "
							+       ",削除ＦＧ = '0' \n"
							;
						if(s削除ＦＧ == "1"){
							cmdQuery
								+=  ",登録日時 = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
								+   ",登録ＰＧ = '" + sData[19] + "' "
								+   ",登録者 = '" + sData[20] + "' \n"
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
								;
							if(s重量入力制御 != "1"){
								cmdQuery = cmdQuery
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
								+   ",才数 = "+ sData[14] +" "
								+   ",重量 = "+ sData[15] +" \n"
								;
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 START
							}
// MOD 2011.05.06 東都）高木 お客様ごとに重量入力制御 END
						}
						cmdQuery
							+=      ",更新日時 = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
							+       ",更新ＰＧ = '" + sData[19] + "' "
							+       ",更新者 = '" + sData[20] + "' \n"
							+ "WHERE 会員ＣＤ = '" + sData[0] + "' \n"
							+   "AND 部門ＣＤ = '" + sData[1] + "' \n"
							+   "AND 荷送人ＣＤ = '" + sData[2] + "' "
							;

							CmdUpdate(sUser, conn2, cmdQuery);
					}
					disposeReader(reader);
					reader = null;
				}
				logWriter(sUser, INF, "正常終了");
				tran.Commit();
			}catch (OracleException ex){
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}catch (Exception ex){
				tran.Rollback();
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}finally{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}
// MOD 2010.09.08 東都）高木 ＣＳＶ取込機能の追加 END
// MOD 2010.09.17 東都）高木 複数件削除機能の追加 START
		/*********************************************************************
		 * 届け先データ削除
		 * 引数：会員ＣＤ、部門ＣＤ、荷受人ＣＤ、更新ＰＧ、更新者
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Del_irainusis(string[] sUser, string[] sData, string[] sList)
		{
			logWriter(sUser, INF, "依頼主複数件削除開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[sList.Length + 1];
			sRet[0] = "";
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null){
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try{
				string cmdQuery; 
// MOD 2011.01.18 東都）高木 複数件削除機能の障害対応 START
				int  iCntUnDelete = 0;
// MOD 2011.01.18 東都）高木 複数件削除機能の障害対応 END
				for(int iCnt = 0; iCnt < sList.Length; iCnt++){
					sRet[iCnt + 1] = "";
					if(sList[iCnt] == null) continue;
					if(sList[iCnt].Length == 0) continue;
// MOD 2011.01.18 東都）高木 複数件削除機能の障害対応 START
					cmdQuery 
					= "SELECT COUNT(*) \n" 
					+   "FROM \"ＳＴ０１出荷ジャーナル\" \n"
					+ " WHERE 会員ＣＤ           = '" + sData[0] +"' \n"
					+ "   AND 部門ＣＤ           = '" + sData[1] +"' \n"
					+ "   AND 荷送人ＣＤ         = '" + sList[iCnt] +"'"
					+ "   AND 削除ＦＧ           = '0' \n";
					OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
					reader.Read();
					string sCnt = reader.GetDecimal(0).ToString().Trim();
					reader.Close();
					disposeReader(reader);
					reader = null;
					if(sCnt != "0"){
						iCntUnDelete++;
						continue;
					}
// MOD 2011.01.18 東都）高木 複数件削除機能の障害対応 END
					cmdQuery 
						= "UPDATE ＳＭ０１荷送人 \n"
						+    "SET 削除ＦＧ           = '1', \n"
						+        "更新ＰＧ           = '" + sData[3] +"', \n"
						+        "更新者             = '" + sData[4] +"', \n"
						+        "更新日時           =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
						+ " WHERE 会員ＣＤ           = '" + sData[0] +"' \n"
						+ "   AND 部門ＣＤ           = '" + sData[1] +"' \n"
						+ "   AND 荷送人ＣＤ         = '" + sList[iCnt] +"'";
					int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
					sRet[iCnt + 1] = iUpdRow.ToString();
				}

				tran.Commit();				
				sRet[0] = "正常終了";
// MOD 2011.01.18 東都）高木 複数件削除機能の障害対応 START
				if(iCntUnDelete > 0){
					sRet[0] = "出荷データが存在するため削除できません[ "+iCntUnDelete+"件]";
				}
// MOD 2011.01.18 東都）高木 複数件削除機能の障害対応 END
			}catch (OracleException ex){
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}catch (Exception ex){
				tran.Rollback();
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}finally{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			
			return sRet;
		}
// MOD 2010.09.17 東都）高木 複数件削除機能の追加 END
	}
}
