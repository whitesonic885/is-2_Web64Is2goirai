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
	// �C������
	//--------------------------------------------------------------------------
	// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j��
	//	disposeReader(reader);
	//	reader = null;
	// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
	//	logFileOpen(sUser);
	//	userCheck2(conn2, sUser);
	//	logFileClose();
	//--------------------------------------------------------------------------
	// MOD 2008.09.18 ���s�j���� �ꗗ�̃\�[�g����[�ב��l�b�c]��ǉ� 
	// MOD 2008.10.01 ���s�j���� �ꗗ�ɐ������\�� 
	//--------------------------------------------------------------------------
	// MOD 2009.11.30 ���s�j���� ���������ɖ��O�A�������ǉ� 
	//--------------------------------------------------------------------------
	// MOD 2010.09.08 ���s�j���� �b�r�u�o�͋@�\�̒ǉ� 
	// MOD 2010.09.08 ���s�j���� �b�r�u�捞�@�\�̒ǉ� 
	// MOD 2010.09.17 ���s�j���� �������폜�@�\�̒ǉ� 
	// MOD 2010.09.27 ���s�j���� �����敔�ۖ��̒ǉ� 
	// MOD 2010.09.29 ���s�j���� �X�֔ԍ�(__)�Ή��i�������o�O���������j 
	//--------------------------------------------------------------------------
	// MOD 2011.01.18 ���s�j���� �������폜�@�\�̏�Q�Ή� 
	// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� 
	// MOD 2011.03.09 ���s�j���� ������}�X�^�̎�L�[��[����b�c]��ǉ� 
	// MOD 2011.04.13 ���s�j���� �d�ʓ��͕s�Ή� 
	// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� 
	//--------------------------------------------------------------------------
	// MOD 2012.09.06 COA)���R Oracle�T�[�o���׌y���΍�iSQL�Ƀo�C���h�ϐ��𗘗p�j
	//--------------------------------------------------------------------------

	[System.Web.Services.WebService(
		 Namespace="http://Walkthrough/XmlWebServices/",
		 Description="is2goirai")]

	public class Service1 : is2common.CommService
	{
		private static string sSepa = "|";
// MOD 2010.09.08 ���s�j���� �b�r�u�o�͋@�\�̒ǉ� START
//		private static string sCRLF = "\\r\\n";
		private static string sKanma = ",";
		private static string sDbl = "\"";
		private static string sSng = "'";
// MOD 2010.09.08 ���s�j���� �b�r�u�o�͋@�\�̒ǉ� END

		public Service1()
		{
			//CODEGEN: ���̌Ăяo���́AASP.NET Web �T�[�r�X �f�U�C�i�ŕK�v�ł��B
			InitializeComponent();

			connectService();
		}

		#region �R���|�[�l���g �f�U�C�i�Ő������ꂽ�R�[�h 
		
		//Web �T�[�r�X �f�U�C�i�ŕK�v�ł��B
		private IContainer components = null;
				
		/// <summary>
		/// �f�U�C�i �T�|�[�g�ɕK�v�ȃ��\�b�h�ł��B���̃��\�b�h�̓��e��
		/// �R�[�h �G�f�B�^�ŕύX���Ȃ��ł��������B
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// �g�p����Ă��郊�\�[�X�Ɍ㏈�������s���܂��B
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
		 * �˗���ꗗ�擾
		 * �����F����b�c�A����b�c�A�J�i�A�ב��l�b�c
		 * �ߒl�F�X�e�[�^�X�A�ꗗ�i���O�P�A�Z���P�A�ב��l�b�c�A�d�b�ԍ��A�J�i���́j
		 *
		 *********************************************************************/
// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H START
//		private static string GET_GOIRAI_SELECT
//			= "SELECT ���O�P,�Z���P,�ב��l�b�c,'(' || TRIM(�d�b�ԍ��P) || ')' \n"
//			+       " || TRIM(�d�b�ԍ��Q) || '-' || TRIM(�d�b�ԍ��R),�J�i���� \n"
//			+ " FROM �r�l�O�P�ב��l \n";
// MOD 2008.10.01 ���s�j���� �ꗗ�ɐ������\�� START
//		private static string GET_GOIRAI_SELECT
//			= "SELECT ���O�P, �Z���P, �ב��l�b�c, �d�b�ԍ��P, \n"
//			+       " �d�b�ԍ��Q, �d�b�ԍ��R, �J�i���� \n"
//			+ " FROM �r�l�O�P�ב��l \n";
		private static string GET_GOIRAI_SELECT_1
			= "SELECT SM01W.���O�P, SM01W.�Z���P, SM01W.�ב��l�b�c, SM01W.�d�b�ԍ��P \n"
			+      ", SM01W.�d�b�ԍ��Q, SM01W.�d�b�ԍ��R, SM01W.�J�i���� \n"
			+      ", NVL(SM04.���Ӑ敔�ۖ�, SM01W.�X�֔ԍ� || ' ' || SM01W.���Ӑ�b�c || ' ' || SM01W.���Ӑ敔�ۂb�c) \n"
// MOD 2009.11.30 ���s�j���� ���������ɖ��O�A�������ǉ� START
			+      ", SM01W.���O�Q, SM01W.�o�^����, SM01W.�X�V���� \n"
// MOD 2009.11.30 ���s�j���� ���������ɖ��O�A�������ǉ� END
			+  " FROM ( \n"
			+      "SELECT SM01.���O�P, SM01.�Z���P, SM01.�ב��l�b�c, SM01.�d�b�ԍ��P \n"
			+      ", SM01.�d�b�ԍ��Q, SM01.�d�b�ԍ��R, SM01.�J�i���� \n"
			+      ", SM01.���Ӑ�b�c, SM01.���Ӑ敔�ۂb�c \n"
			+      ", CM02.�X�֔ԍ� \n"
// MOD 2009.11.30 ���s�j���� ���������ɖ��O�A�������ǉ� START
			+      ", SM01.���O�Q, SM01.�o�^����, SM01.�X�V���� \n"
// MOD 2009.11.30 ���s�j���� ���������ɖ��O�A�������ǉ� END
// MOD 2011.03.09 ���s�j���� ������}�X�^�̎�L�[��[����b�c]��ǉ� START
			+      ", SM01.����b�c \n"
// MOD 2011.03.09 ���s�j���� ������}�X�^�̎�L�[��[����b�c]��ǉ� END
			+ " FROM �r�l�O�P�ב��l SM01 \n"
			+     ", �b�l�O�Q����   CM02 \n"
			;
		private static string GET_GOIRAI_SELECT_2
			=     ") SM01W \n"
			+     ", �r�l�O�S������ SM04 \n"
			+     " WHERE SM01W.�X�֔ԍ� = SM04.�X�֔ԍ�(+) \n"
			+       " AND SM01W.���Ӑ�b�c = SM04.���Ӑ�b�c(+) \n"
			+       " AND SM01W.���Ӑ敔�ۂb�c = SM04.���Ӑ敔�ۂb�c(+) \n"
// MOD 2011.03.09 ���s�j���� ������}�X�^�̎�L�[��[����b�c]��ǉ� START
			+       " AND SM01W.����b�c = SM04.����b�c(+) \n"
// MOD 2011.03.09 ���s�j���� ������}�X�^�̎�L�[��[����b�c]��ǉ� END
			+       " AND '0' = SM04.�폜�e�f(+) \n"
// MOD 2009.11.30 ���s�j���� ���������ɖ��O�A�������ǉ� START
//			+     " ORDER BY SM01W.���O�P, SM01W.�ב��l�b�c \n"
// MOD 2009.11.30 ���s�j���� ���������ɖ��O�A�������ǉ� END
			;
// MOD 2008.10.01 ���s�j���� �ꗗ�ɐ������\�� END
// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H END

		[WebMethod]
		public String[] Get_irainusi(string[] sUser, string sKCode, string sBCode, string sKana, string sICode)
		{
// MOD 2009.11.30 ���s�j���� ���������ɖ��O�A�������ǉ� START
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
			string s���O   = "";
			string s������ = "";
			string s����   = "";
			int i�K�w���X�g�P = 0;
			int i�\�[�g�����P = 0;
			int i�K�w���X�g�Q = 0;
			int i�\�[�g�����Q = 0;

			sKCode = sKey[0];
			sBCode = sKey[1];
			sKana  = sKey[2];
			sICode = sKey[3];
			if(sKey.Length > 4){
				s���O   = sKey[4];
				s������ = sKey[5];
				s����   = sKey[6];
				try{
					i�K�w���X�g�P = int.Parse(sKey[7]);
					i�\�[�g�����P = int.Parse(sKey[8]);
					i�K�w���X�g�Q = int.Parse(sKey[9]);
					i�\�[�g�����Q = int.Parse(sKey[10]);
				}catch(Exception){
					;
				}
			}
// MOD 2009.11.30 ���s�j���� ���������ɖ��O�A�������ǉ� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//			logFileOpen(sUser);
			logWriter(sUser, INF, "�˗���ꗗ�擾�J�n");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//// ADD 2005.05.23 ���s�j�����J ����`�F�b�N�ǉ� START
//			// ����`�F�b�N
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 ���s�j�����J ����`�F�b�N�ǉ� END

			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbRet = new StringBuilder(1024);
			try
			{
// MOD 2008.10.01 ���s�j���� �ꗗ�ɐ������\�� START
//				sbQuery.Append(GET_GOIRAI_SELECT);
//				sbQuery.Append(" WHERE ����b�c = '" + sKCode + "' \n");
//				sbQuery.Append("   AND ����b�c = '" + sBCode + "' \n");
//
//				if(sKana.Length > 0 && sICode.Length == 0)
//				{
//					sbQuery.Append(" AND �J�i���� LIKE '"+ sKana + "%' \n");
//				}
//				if(sICode.Length > 0 && sKana.Length == 0)
//				{
//					sbQuery.Append(" AND �ב��l�b�c LIKE '"+ sICode + "%' \n");
//				}
//				if(sICode.Length > 0 && sKana.Length > 0)
//				{
//					sbQuery.Append(" AND �J�i���� LIKE '"+ sKana + "%' \n");
//					sbQuery.Append(" AND �ב��l�b�c LIKE '"+ sICode + "%' \n");
//				}
//				sbQuery.Append(" AND �폜�e�f = '0' \n");
//// MOD 2008.09.18 ���s�j���� �ꗗ�̃\�[�g����[�ב��l�b�c]��ǉ� START
////				sbQuery.Append(" ORDER BY ���O�P \n");
//				sbQuery.Append(" ORDER BY ���O�P, �ב��l�b�c \n");
//// MOD 2008.09.18 ���s�j���� �ꗗ�̃\�[�g����[�ב��l�b�c]��ǉ� END
				sbQuery.Append(GET_GOIRAI_SELECT_1);
				sbQuery.Append(" WHERE SM01.����b�c = '" + sKCode + "' \n");
				sbQuery.Append("   AND SM01.����b�c = '" + sBCode + "' \n");
				sbQuery.Append("   AND SM01.����b�c = CM02.����b�c \n");
				sbQuery.Append("   AND SM01.����b�c = CM02.����b�c \n");

				if(sKana.Length > 0 && sICode.Length == 0)
				{
					sbQuery.Append(" AND SM01.�J�i���� LIKE '"+ sKana + "%' \n");
				}
				if(sICode.Length > 0 && sKana.Length == 0)
				{
					sbQuery.Append(" AND SM01.�ב��l�b�c LIKE '"+ sICode + "%' \n");
				}
				if(sICode.Length > 0 && sKana.Length > 0)
				{
					sbQuery.Append(" AND SM01.�J�i���� LIKE '"+ sKana + "%' \n");
					sbQuery.Append(" AND SM01.�ב��l�b�c LIKE '"+ sICode + "%' \n");
				}
// MOD 2009.11.30 ���s�j���� ���������ɖ��O�A�������ǉ� START
				if(s���O.Length > 0){
					sbQuery.Append(" AND SM01.���O�P LIKE '%"+ s���O + "%' \n");
				}
				if(s������.Length > 0){
					sbQuery.Append(" AND SM01.���Ӑ�b�c = '"+ s������ + "' \n");
					if(s����.Length > 0){
						sbQuery.Append(" AND SM01.���Ӑ敔�ۂb�c = '"+ s���� + "' \n");
					}else{
						sbQuery.Append(" AND SM01.���Ӑ敔�ۂb�c = ' ' \n");
					}
				}
// MOD 2009.11.30 ���s�j���� ���������ɖ��O�A�������ǉ� END
				sbQuery.Append(" AND SM01.�폜�e�f = '0' \n");
				sbQuery.Append(GET_GOIRAI_SELECT_2);
// MOD 2008.10.01 ���s�j���� �ꗗ�ɐ������\�� END

// MOD 2009.11.30 ���s�j���� ���������ɖ��O�A�������ǉ� START
				sbQuery.Append(" ORDER BY \n");
				switch(i�K�w���X�g�P){
				case 1:
					sbQuery.Append(" SM01W.�J�i���� ");
					if(i�\�[�g�����P == 1) sbQuery.Append(" DESC ");
					break;
				case 2:
					sbQuery.Append(" SM01W.�ב��l�b�c");
					if(i�\�[�g�����P == 1) sbQuery.Append(" DESC ");
					break;
				case 3:
//					sbQuery.Append(" SM01W.���Ӑ�b�c ");
//					if(i�\�[�g�����P == 1) sbQuery.Append(" DESC ");
//					sbQuery.Append(", SM01W.���Ӑ敔�ۂb�c ");
					sbQuery.Append(" NVL(SM04.���Ӑ敔�ۖ�,SM01W.���Ӑ�b�c || SM01W.���Ӑ敔�ۂb�c) ");
					if(i�\�[�g�����P == 1) sbQuery.Append(" DESC ");
					break;
				case 4:
					sbQuery.Append(" SM01W.���O�P ");
					if(i�\�[�g�����P == 1) sbQuery.Append(" DESC ");
//					sbQuery.Append(", SM01W.���O�Q ");
//					if(i�\�[�g�����P == 1) sbQuery.Append(" DESC ");
					break;
				case 5:
					sbQuery.Append(" SM01W.�d�b�ԍ��P ");
					if(i�\�[�g�����P == 1) sbQuery.Append(" DESC ");
					sbQuery.Append(", SM01W.�d�b�ԍ��Q ");
					if(i�\�[�g�����P == 1) sbQuery.Append(" DESC ");
					sbQuery.Append(", SM01W.�d�b�ԍ��R ");
					if(i�\�[�g�����P == 1) sbQuery.Append(" DESC ");
					break;
				case 6:
					sbQuery.Append(" SM01W.�o�^���� ");
					if(i�\�[�g�����P == 1) sbQuery.Append(" DESC ");
					break;
				case 7:
					sbQuery.Append(" SM01W.�X�V���� ");
					if(i�\�[�g�����P == 1) sbQuery.Append(" DESC ");
					break;
				}
				if(i�K�w���X�g�P != 0 && i�K�w���X�g�Q != 0){
					sbQuery.Append(",");
				}
				switch(i�K�w���X�g�Q){
				case 1:
					sbQuery.Append(" SM01W.�J�i���� ");
					if(i�\�[�g�����Q == 1) sbQuery.Append(" DESC ");
					break;
				case 2:
					sbQuery.Append(" SM01W.�ב��l�b�c");
					if(i�\�[�g�����Q == 1) sbQuery.Append(" DESC ");
					break;
				case 3:
//					sbQuery.Append(" SM01W.���Ӑ�b�c ");
//					if(i�\�[�g�����Q == 1) sbQuery.Append(" DESC ");
//					sbQuery.Append(", SM01W.���Ӑ敔�ۂb�c ");
					sbQuery.Append(" NVL(SM04.���Ӑ敔�ۖ�,SM01W.���Ӑ�b�c || SM01W.���Ӑ敔�ۂb�c) ");
					if(i�\�[�g�����Q == 1) sbQuery.Append(" DESC ");
					break;
				case 4:
					sbQuery.Append(" SM01W.���O�P ");
					if(i�\�[�g�����Q == 1) sbQuery.Append(" DESC ");
//					sbQuery.Append(", SM01W.���O�Q ");
//					if(i�\�[�g�����Q == 1) sbQuery.Append(" DESC ");
					break;
				case 5:
					sbQuery.Append(" SM01W.�d�b�ԍ��P ");
					if(i�\�[�g�����Q == 1) sbQuery.Append(" DESC ");
					sbQuery.Append(", SM01W.�d�b�ԍ��Q ");
					if(i�\�[�g�����Q == 1) sbQuery.Append(" DESC ");
					sbQuery.Append(", SM01W.�d�b�ԍ��R ");
					if(i�\�[�g�����Q == 1) sbQuery.Append(" DESC ");
					break;
				case 6:
					sbQuery.Append(" SM01W.�o�^���� ");
					if(i�\�[�g�����Q == 1) sbQuery.Append(" DESC ");
					break;
				case 7:
					sbQuery.Append(" SM01W.�X�V���� ");
					if(i�\�[�g�����Q == 1) sbQuery.Append(" DESC ");
					break;
				}
				if(i�K�w���X�g�P == 0 && i�K�w���X�g�Q == 0){
					sbQuery.Append(" SM01W.���O�P \n");
				}
				if(i�K�w���X�g�P != 2 && i�K�w���X�g�Q != 2){
					sbQuery.Append(", SM01W.�ב��l�b�c \n");
				}
// MOD 2009.11.30 ���s�j���� ���������ɖ��O�A�������ǉ� END
				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);

				while (reader.Read())
				{
					sbRet = new StringBuilder(1024);

// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� START
//					sbRet.Append(sSepa + reader.GetString(0).Trim());
//					sbRet.Append(sSepa + reader.GetString(1).Trim());
					sbRet.Append(sSepa + reader.GetString(0).TrimEnd()); // ���O�P
					sbRet.Append(sSepa + reader.GetString(1).TrimEnd()); // �Z���P
// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� END
					sbRet.Append(sSepa + reader.GetString(2).Trim());
// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H START
//					sbRet.Append(sSepa + reader.GetString(3));
//					sbRet.Append(sSepa + reader.GetString(4).Trim());
					sbRet.Append(sSepa + "(" + reader.GetString(3).Trim() + ")"
										+ reader.GetString(4).Trim() + "-"
										+ reader.GetString(5).Trim());	// �d�b�ԍ�
// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� START
//					sbRet.Append(sSepa + reader.GetString(6).Trim()); // �J�i����
					sbRet.Append(sSepa + reader.GetString(6).TrimEnd());// �J�i����
// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� END
// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H END
// ADD 2008.10.01 ���s�j���� �ꗗ�ɐ������\�� START
					sbRet.Append(sSepa + reader.GetString(7).Trim());	// �˗���
// ADD 2008.10.01 ���s�j���� �ꗗ�ɐ������\�� END

					sList.Add(sbRet);
				}
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
				sRet = new string[sList.Count + 1];

				if (sList.Count == 0)
					sRet[0] = "�Y���f�[�^������܂���";
				else
				{
					sRet[0] = "����I��";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				conn2 = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
			}
			
			return sRet;
		}

		/*********************************************************************
		 * �˗���f�[�^�擾
		 * �����F����b�c�A����b�c�A�ב��l�b�c
		 * �ߒl�F�X�e�[�^�X�A�J�i���́A�d�b�ԍ��A�X�֔ԍ��A�Z���A���O�A�d��
		 *		 ���[���A�h���X�A���Ӑ�b�c�A���Ӑ敔�ۂb�c�A�X�V����
		 *********************************************************************/
// MOD 2011.01.18 ���s�j���� �������폜�@�\�̏�Q�Ή� START
//// ADD 2005.05.11 ���s�j���� ORA-03113�΍�H START
//		private static string GET_SIRAINUSI_SELECT
//			= "SELECT �J�i����, �d�b�ԍ��P, �d�b�ԍ��Q, �d�b�ԍ��R, �X�֔ԍ�, \n"
//			+ " �Z���P, �Z���Q, ���O�P, ���O�Q, �ː�, \n"
//			+ " �d��, \"���[���A�h���X\", ���Ӑ�b�c, ���Ӑ敔�ۂb�c, �X�V���� \n"
//			+ " FROM �r�l�O�P�ב��l \n";
//// ADD 2005.05.11 ���s�j���� ORA-03113�΍�H END
// MOD 2011.01.18 ���s�j���� �������폜�@�\�̏�Q�Ή� END

		[WebMethod]
		public String[] Get_Sirainusi(string[] sUser, string sKCode,string sBCode,string sICode)
		{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//			logFileOpen(sUser);
			logWriter(sUser, INF, "�˗�����擾�J�n");

			OracleConnection conn2 = null;
// MOD 2010.09.27 ���s�j���� �����敔�ۖ��̒ǉ� START
//			string[] sRet = new string[18];
			string[] sRet = new string[19];
// MOD 2010.09.27 ���s�j���� �����敔�ۖ��̒ǉ� END

			// ADD-S 2012.09.06 COA)���R Oracle�T�[�o���׌y���΍�iSQL�Ƀo�C���h�ϐ��𗘗p�j
			OracleParameter[]	wk_opOraParam	= null;
			// ADD-E 2012.09.06 COA)���R Oracle�T�[�o���׌y���΍�iSQL�Ƀo�C���h�ϐ��𗘗p�j

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//// ADD 2005.05.23 ���s�j�����J ����`�F�b�N�ǉ� START
//			// ����`�F�b�N
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 ���s�j�����J ����`�F�b�N�ǉ� END

// MOD 2010.09.27 ���s�j���� �����敔�ۖ��̒ǉ� START
			string s����b�c   = sKCode;
			string s����b�c   = sBCode;
			string s�˗���b�c = sICode;
			StringBuilder sbQuery = new StringBuilder(1024);
// MOD 2010.09.27 ���s�j���� �����敔�ۖ��̒ǉ� END
			try
			{
// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H START
//				string cmdQuery = "SELECT �J�i����,�d�b�ԍ��P,�d�b�ԍ��Q,�d�b�ԍ��R, \n"
//					+ " substr(�X�֔ԍ�,1,3),substr(�X�֔ԍ�,4,4),�Z���P,�Z���Q,���O�P,���O�Q, \n"
//					+ " TO_CHAR(�ː�),TO_CHAR(�d��), \"���[���A�h���X\",���Ӑ�b�c,���Ӑ敔�ۂb�c, \n"
//					+ " TO_CHAR(�X�V����) \n"
//					+ " FROM �r�l�O�P�ב��l \n"
//					+ " WHERE �ב��l�b�c = '" + sICode + "' AND ����b�c = '" + sKCode + "' \n"
//					+  "  AND ����b�c   = '" + sBCode + "' AND �폜�e�f = '0'";
// MOD 2010.09.27 ���s�j���� �����敔�ۖ��̒ǉ� START
//				string cmdQuery = GET_SIRAINUSI_SELECT
//					+ " WHERE �ב��l�b�c = '" + sICode + "' \n"
//					+ " AND ����b�c = '" + sKCode + "' \n"
//					+ " AND ����b�c = '" + sBCode + "' \n"
//					+ " AND �폜�e�f = '0' \n";
//// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H END
//				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
//
				sbQuery.Append("SELECT SM01.�J�i���� \n");
				sbQuery.Append(      ",SM01.�d�b�ԍ��P \n");
				sbQuery.Append(      ",SM01.�d�b�ԍ��Q \n");
				sbQuery.Append(      ",SM01.�d�b�ԍ��R \n");
				sbQuery.Append(      ",SM01.�X�֔ԍ� \n");
				sbQuery.Append(      ",SM01.�Z���P \n");
				sbQuery.Append(      ",SM01.�Z���Q \n");
				sbQuery.Append(      ",SM01.���O�P \n");
				sbQuery.Append(      ",SM01.���O�Q \n");
				sbQuery.Append(      ",SM01.�ː� \n");
				sbQuery.Append(      ",SM01.�d�� \n");
				sbQuery.Append(      ",SM01.\"���[���A�h���X\" \n");
				sbQuery.Append(      ",SM01.���Ӑ�b�c \n");
				sbQuery.Append(      ",SM01.���Ӑ敔�ۂb�c \n");
				sbQuery.Append(      ",SM01.�X�V���� \n");
				sbQuery.Append(      ",NVL(SM04.���Ӑ敔�ۖ�,' ') \n");
				sbQuery.Append( "FROM �r�l�O�P�ב��l SM01 \n");
				sbQuery.Append(" LEFT JOIN \"�b�l�O�Q����\" CM02 \n");
				sbQuery.Append(" ON SM01.����b�c = CM02.����b�c ");
				sbQuery.Append("AND SM01.����b�c = CM02.����b�c ");
				sbQuery.Append("AND CM02.�폜�e�f = '0' \n");
				sbQuery.Append(" LEFT JOIN \"�r�l�O�S������\" SM04 \n");
				sbQuery.Append(" ON CM02.�X�֔ԍ� = SM04.�X�֔ԍ� ");
				sbQuery.Append("AND SM01.���Ӑ�b�c = SM04.���Ӑ�b�c ");
				sbQuery.Append("AND SM01.���Ӑ敔�ۂb�c = SM04.���Ӑ敔�ۂb�c ");
// MOD 2011.03.09 ���s�j���� ������}�X�^�̎�L�[��[����b�c]��ǉ� START
				sbQuery.Append("AND SM01.����b�c = SM04.����b�c ");
// MOD 2011.03.09 ���s�j���� ������}�X�^�̎�L�[��[����b�c]��ǉ� END
				sbQuery.Append("AND SM04.�폜�e�f = '0' \n");
				sbQuery.Append("WHERE SM01.����b�c = '" + s����b�c + "' \n");
				sbQuery.Append(  "AND SM01.����b�c = '" + s����b�c + "' \n");
				sbQuery.Append(  "AND SM01.�ב��l�b�c = '" + s�˗���b�c + "' \n");
				sbQuery.Append(  "AND SM01.�폜�e�f = '0' \n");

				// MOD-S 2012.09.06 COA)���R Oracle�T�[�o���׌y���΍�iSQL�Ƀo�C���h�ϐ��𗘗p�j
				//OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);
				logWriter(sUser, INF_SQL, "###�o�C���h��i�z��j###\n" + sbQuery.ToString());	//�C���O��UPDATE�������O�o��

				sbQuery = new StringBuilder(1024);
				sbQuery.Append("SELECT SM01.�J�i���� \n");
				sbQuery.Append(      ",SM01.�d�b�ԍ��P \n");
				sbQuery.Append(      ",SM01.�d�b�ԍ��Q \n");
				sbQuery.Append(      ",SM01.�d�b�ԍ��R \n");
				sbQuery.Append(      ",SM01.�X�֔ԍ� \n");
				sbQuery.Append(      ",SM01.�Z���P \n");
				sbQuery.Append(      ",SM01.�Z���Q \n");
				sbQuery.Append(      ",SM01.���O�P \n");
				sbQuery.Append(      ",SM01.���O�Q \n");
				sbQuery.Append(      ",SM01.�ː� \n");
				sbQuery.Append(      ",SM01.�d�� \n");
				sbQuery.Append(      ",SM01.\"���[���A�h���X\" \n");
				sbQuery.Append(      ",SM01.���Ӑ�b�c \n");
				sbQuery.Append(      ",SM01.���Ӑ敔�ۂb�c \n");
				sbQuery.Append(      ",SM01.�X�V���� \n");
				sbQuery.Append(      ",NVL(SM04.���Ӑ敔�ۖ�,' ') \n");
				sbQuery.Append( "FROM �r�l�O�P�ב��l SM01 \n");
				sbQuery.Append(" LEFT JOIN \"�b�l�O�Q����\" CM02 \n");
				sbQuery.Append(" ON SM01.����b�c = CM02.����b�c ");
				sbQuery.Append("AND SM01.����b�c = CM02.����b�c ");
				sbQuery.Append("AND CM02.�폜�e�f = '0' \n");
				sbQuery.Append(" LEFT JOIN \"�r�l�O�S������\" SM04 \n");
				sbQuery.Append(" ON CM02.�X�֔ԍ� = SM04.�X�֔ԍ� ");
				sbQuery.Append("AND SM01.���Ӑ�b�c = SM04.���Ӑ�b�c ");
				sbQuery.Append("AND SM01.���Ӑ敔�ۂb�c = SM04.���Ӑ敔�ۂb�c ");
				sbQuery.Append("AND SM01.����b�c = SM04.����b�c ");
				sbQuery.Append("AND SM04.�폜�e�f = '0' \n");
				sbQuery.Append("WHERE SM01.����b�c   = :p_KaiinCD \n");
				sbQuery.Append(  "AND SM01.����b�c   = :p_BumonCD \n");
				sbQuery.Append(  "AND SM01.�ב��l�b�c = :p_NisouCD \n");
				sbQuery.Append(  "AND SM01.�폜�e�f = '0' \n");

				wk_opOraParam = new OracleParameter[3];
				wk_opOraParam[0] = new OracleParameter("p_KaiinCD", OracleDbType.Char, s����b�c,   ParameterDirection.Input);
				wk_opOraParam[1] = new OracleParameter("p_BumonCD", OracleDbType.Char, s����b�c,   ParameterDirection.Input);
				wk_opOraParam[2] = new OracleParameter("p_NisouCD", OracleDbType.Char, s�˗���b�c, ParameterDirection.Input);

				OracleDataReader	reader = CmdSelect(sUser, conn2, sbQuery, wk_opOraParam);
				wk_opOraParam = null;
				// MOD-E 2012.09.06 COA)���R Oracle�T�[�o���׌y���΍�iSQL�Ƀo�C���h�ϐ��𗘗p�j
// MOD 2010.09.27 ���s�j���� �����敔�ۖ��̒ǉ� END

				bool bRead = reader.Read();
				if(bRead == true)
				{
// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H START
//					for(int iCnt = 1; iCnt < 17; iCnt++)
//					{
//						sRet[iCnt] = reader.GetString(iCnt - 1).Trim();
//					}
// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� START
//					sRet[1]  = reader.GetString(0).Trim();
					sRet[1]  = reader.GetString(0).TrimEnd();
// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� END
					sRet[2]  = reader.GetString(1).Trim();
					sRet[3]  = reader.GetString(2).Trim();
					sRet[4]  = reader.GetString(3).Trim();
					sRet[5]  = reader.GetString(4).Trim();	// �X�֔ԍ�
					sRet[6]  = "";
					if(sRet[5].Length > 3)
					{
						sRet[6]  = sRet[5].Substring(3);
// MOD 2005.09.02 ���s�j�����J Trim�ǉ� START
//						sRet[5]  = sRet[5].Substring(0,3);
						sRet[5]  = sRet[5].Substring(0,3).Trim();
// MOD 2005.09.02 ���s�j�����J Trim�ǉ� END
					}
// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� START
//					sRet[7]  = reader.GetString(5).Trim();
//					sRet[8]  = reader.GetString(6).Trim();
//					sRet[9]  = reader.GetString(7).Trim();
//					sRet[10] = reader.GetString(8).Trim();
					sRet[7]  = reader.GetString(5).TrimEnd(); // �Z���P
					sRet[8]  = reader.GetString(6).TrimEnd(); // �Z���Q
					sRet[9]  = reader.GetString(7).TrimEnd(); // ���O�P
					sRet[10] = reader.GetString(8).TrimEnd(); // ���O�Q
// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� END
					sRet[11] = reader.GetDecimal(9).ToString().Trim();	// �ː�
					sRet[12] = reader.GetDecimal(10).ToString().Trim();	// �d��
					sRet[13] = reader.GetString(11).Trim();
					sRet[14] = reader.GetString(12).Trim();
					sRet[15] = reader.GetString(13).Trim();
					sRet[16] = reader.GetDecimal(14).ToString().Trim();	// �X�V����

					sRet[0] = "�X�V";
					sRet[17] = "U";
// MOD 2010.09.27 ���s�j���� �����敔�ۖ��̒ǉ� START
					sRet[18] = reader.GetString(15).TrimEnd(); // ���Ӑ敔�ۖ�
// MOD 2010.09.27 ���s�j���� �����敔�ۖ��̒ǉ� END
				}
				else
				{
					sRet[0] = "�o�^";
					sRet[17] = "I";
				}
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END

				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				conn2 = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
			}
			
			return sRet;
		}

		/*********************************************************************
		 * �˗���f�[�^�X�V
		 * �����F����b�c�A����b�c�A�ב��l�b�c...
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Upd_irainusi(string[] sUser, string[] sData)
		{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//			logFileOpen(sUser);
			logWriter(sUser, INF, "�˗���X�V�J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[5];
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//// ADD 2005.05.23 ���s�j�����J ����`�F�b�N�ǉ� START
//			// ����`�F�b�N
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 ���s�j�����J ����`�F�b�N�ǉ� END

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				string cmdQuery 
					= "UPDATE �r�l�O�P�ב��l \n"
					+    "SET �J�i����           = '" + sData[1]  +"', \n"
					+        "�d�b�ԍ��P         = '" + sData[2]  +"', \n"
					+        "�d�b�ԍ��Q         = '" + sData[3]  +"', \n"
					+        "�d�b�ԍ��R         = '" + sData[4]  +"', \n"
					+        "�X�֔ԍ�           = '" + sData[5]  + sData[6] + "', \n"
					+        "�Z���P             = '" + sData[7]  +"', \n"
					+        "�Z���Q             = '" + sData[8]  +"', \n"
					+        "���O�P             = '" + sData[9]  +"', \n"
					+        "���O�Q             = '" + sData[10] +"', \n"
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
					;
				string s�d�ʓ��͐��� = (sData.Length > 23) ? sData[23] : "0";
				if(s�d�ʓ��͐��� == "1"){
					cmdQuery = cmdQuery
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
// MOD 2005.05.17 ���s�j�����J �ː����� START
					+        "�ː�               =  " + sData[11] +", \n"
// MOD 2005.05.17 ���s�j�����J �ː����� END
					+        "�d��               =  " + sData[12] +", \n"
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
					;
				}
				cmdQuery = cmdQuery
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
					+        "\"���[���A�h���X\" = '" + sData[13] +"', \n"
					+        "���Ӑ�b�c         = '" + sData[19] +"', \n"
					+        "���Ӑ敔�ۂb�c     = '" + sData[20] +"', \n"
					+        "�폜�e�f           = '0', \n"
					+        "�X�V�o�f           = '" + sData[14] +"', \n"
					+        "�X�V��             = '" + sData[15] +"', \n"
					+        "�X�V����           =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
					+ " WHERE ����b�c           = '" + sData[16] +"' \n"
					+ "   AND ����b�c           = '" + sData[17] +"' \n"
					+ "   AND �ב��l�b�c         = '" + sData[0] +"' \n"
					+ "   AND �X�V����           =  " + sData[18] +"";

				int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);

				if(iUpdRow == 0)
					sRet[0] = "�f�[�^�ҏW���ɑ��̒[�����X�V����Ă��܂��B\r\n�ēx�A�ŐV�f�[�^���Ăяo���čX�V���Ă��������B";
				else
				{
					sRet[0] = "����I��";
					if(sData[21] == "1")
					{
						bool riyou = Upd_riyou(sUser, conn2, sData[16], sData[15], sData[0], sData[14]);
						if(riyou == false)
							sRet[0] = "�f�t�H���g�ɐݒ�ł��܂���ł���";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				conn2 = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
			}
			
			return sRet;
		}

		/*********************************************************************
		 * �˗���f�[�^�o�^
		 * �����F����b�c�A����b�c�A�ב��l�b�c...
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Ins_irainusi(string[] sUser, string[] sData)
		{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//			logFileOpen(sUser);
			logWriter(sUser, INF, "�˗���o�^�J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[5];
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//// ADD 2005.05.23 ���s�j�����J ����`�F�b�N�ǉ� START
//			// ����`�F�b�N
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 ���s�j�����J ����`�F�b�N�ǉ� END

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
// MOD 2010.09.17 ���s�j���� �������폜�@�\�̒ǉ� START
//				string cmdQuery 
//					= "DELETE FROM �r�l�O�P�ב��l "
//					+ " WHERE ����b�c           = '" + sData[16] +"'"
//					+ "   AND ����b�c           = '" + sData[17] +"'"
//					+ "   AND �ב��l�b�c         = '" + sData[0] +"'"
//					+ "   AND �폜�e�f           = '1'";
//
//				CmdUpdate(sUser, conn2, cmdQuery);
				string cmdQuery = "";
				cmdQuery
					= "SELECT �폜�e�f \n"
					+   "FROM �r�l�O�P�ב��l \n"
					+  "WHERE ����b�c = '" + sData[16] + "' \n"
					+    "AND ����b�c = '" + sData[17] + "' \n"
					+    "AND �ב��l�b�c = '" + sData[0] + "' "
					+    "FOR UPDATE "
					;

				OracleDataReader reader;
				reader = CmdSelect(sUser, conn2, cmdQuery);
				string s�폜�e�f = "";
				if(reader.Read()){
					s�폜�e�f = reader.GetString(0);
				}
				reader.Close();
				disposeReader(reader);
				reader = null;

// MOD 2011.04.13 ���s�j���� �d�ʓ��͕s�Ή� START
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
				string s�d�ʓ��͐��� = (sData.Length > 23) ? sData[23] : "0";
				if(s�d�ʓ��͐��� != "1"){
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
					sData[11] = "0"; // �ː�
					sData[12] = "0"; // �d��
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
				}
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
// MOD 2011.04.13 ���s�j���� �d�ʓ��͕s�Ή� END
				if(s�폜�e�f == "0"){
					sRet[0] = "�f�[�^�ҏW���ɑ��̒[�����X�V����Ă��܂��B\r\n"
							+ "�ēx�A�ŐV�f�[�^���Ăяo���čX�V���Ă��������B";
					tran.Commit();
					logWriter(sUser, INF, sRet[0]);
					return sRet;
				}else if(s�폜�e�f == "1"){
					cmdQuery 
						= "UPDATE �r�l�O�P�ב��l \n"
						+    "SET "
						+        "���Ӑ�b�c         = '" + sData[19] +"', \n"
						+        "���Ӑ敔�ۂb�c     = '" + sData[20] +"', \n"
						+        "�d�b�ԍ��P         = '" + sData[2]  +"', \n"
						+        "�d�b�ԍ��Q         = '" + sData[3]  +"', \n"
						+        "�d�b�ԍ��R         = '" + sData[4]  +"', \n"
						+        "�e�`�w�ԍ��P       = ' ', \n"
						+        "�e�`�w�ԍ��Q       = ' ', \n"
						+        "�e�`�w�ԍ��R       = ' ', \n"
						+        "�Z���P             = '" + sData[7]  +"', \n"
						+        "�Z���Q             = '" + sData[8]  +"', \n"
						+        "�Z���R             = ' ', \n"
						+        "���O�P             = '" + sData[9]  +"', \n"
						+        "���O�Q             = '" + sData[10] +"', \n"
						+        "���O�R             = ' ', \n"
						+        "�X�֔ԍ�           = '" + sData[5]  + sData[6] + "', \n"
						+        "�J�i����           = '" + sData[1]  +"', \n"
						+        "�ː�               =  " + sData[11] +", \n"
						+        "�d��               =  " + sData[12] +", \n"
						+        "�׎D�敪           = ' ', \n"
						+        "\"���[���A�h���X\" = '" + sData[13] +"', \n"
						+        "�폜�e�f           = '0', \n"
						+        "�o�^����           =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), \n"
						+        "�o�^�o�f           = '" + sData[14] +"', \n"
						+        "�o�^��             = '" + sData[15] +"', \n"
						+        "�X�V����           =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), \n"
						+        "�X�V�o�f           = '" + sData[14] +"', \n"
						+        "�X�V��             = '" + sData[15] +"'  \n"
						+ " WHERE ����b�c           = '" + sData[16] +"' \n"
						+ "   AND ����b�c           = '" + sData[17] +"' \n"
						+ "   AND �ב��l�b�c         = '" + sData[0] +"' \n"
						;
				}else{
// MOD 2010.09.17 ���s�j���� �������폜�@�\�̒ǉ� END
					cmdQuery 
						= "INSERT INTO �r�l�O�P�ב��l "
						+ "VALUES ('" + sData[16] +"','" + sData[17] +"','" + sData[0] +"', \n"
						+         "'" + sData[19] +"','" + sData[20] +"','" + sData[2] +"','" + sData[3] +"', \n"
						+         "'" + sData[4] +"',' ',' ',' ','" + sData[7] +"','" + sData[8] +"', \n"
						+         "' ','" + sData[9] +"','" + sData[10] +"',' ','" + sData[5] + sData[6] +"', \n"
						+         "'" + sData[1] +"'," + sData[11] +"," + sData[12] +",' ','" + sData[13] +"', \n"
						+         "'0',TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'),'" + sData[14] +"','" + sData[15] +"', \n"
						+         "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'),'" + sData[14] +"','" + sData[15] +"')";
// MOD 2010.09.17 ���s�j���� �������폜�@�\�̒ǉ� START
				}
// MOD 2010.09.17 ���s�j���� �������폜�@�\�̒ǉ� END

				CmdUpdate(sUser, conn2, cmdQuery);

				sRet[0] = "����I��";

				if(sData[21] == "1")
				{
					bool riyou = Upd_riyou(sUser, conn2, sData[16], sData[15], sData[0], sData[14]);
					if(riyou == false)
						sRet[0] = "�f�t�H���g�ɐݒ�ł��܂���ł���";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				conn2 = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
			}
			
			return sRet;
		}

		/*********************************************************************
		 * �˗���f�[�^�폜
		 * �����F����b�c�A����b�c�A�ב��l�b�c�A�X�V�o�f�A�X�V��
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Del_irainusi(string[] sUser, string[] sData)
		{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//			logFileOpen(sUser);
			logWriter(sUser, INF, "�˗���폜�J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[5];
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//// ADD 2005.05.23 ���s�j�����J ����`�F�b�N�ǉ� START
//			// ����`�F�b�N
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 ���s�j�����J ����`�F�b�N�ǉ� END

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				string cmdQuery 
					= "UPDATE �r�l�O�P�ב��l "
					+    "SET �폜�e�f           = '1', \n"
					+        "�X�V�o�f           = '" + sData[3] +"', \n"
					+        "�X�V��             = '" + sData[4] +"', \n"
					+        "�X�V����           =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
					+ " WHERE ����b�c           = '" + sData[0] +"' \n"
					+ "   AND ����b�c           = '" + sData[1] +"' \n"
					+ "   AND �ב��l�b�c         = '" + sData[2] +"'";

				CmdUpdate(sUser, conn2, cmdQuery);

				tran.Commit();				
				sRet[0] = "����I��";
				
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				conn2 = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
			}
			
			return sRet;
		}

		/*********************************************************************
		 * ���p�҃f�[�^�X�V
		 * �����F����b�c�A���p�҂b�c�A�ב��l�b�c�A�X�V�o�f�A�X�V��
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		private bool Upd_riyou(string[] sUser, OracleConnection conn2, string sKcode, string sRcode, string sIcode, string sPg)
		{
			string cmdQuery
				= "UPDATE �b�l�O�S���p�� "
				+    "SET �ב��l�b�c     = '" + sIcode + "', \n"
				+        "�X�V����       = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), \n"
				+        "�X�V�o�f       = '" + sPg    + "', \n"
				+        "�X�V��         = '" + sRcode + "'  \n"
				+  "WHERE ����b�c       = '" + sKcode + "'  \n"
				+    "AND ���p�҂b�c     = '" + sRcode + "'  ";

			int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);

			if(iUpdRow == 0)
				return false;
			else
				return true;
		}

// ADD 2005.11.07 ���s�j�ɉ� �o�׃W���[�i���`�F�b�N�ǉ� START
		/*********************************************************************
		 * �˗���g�p���`�F�b�N
		 * �����F����b�c�A���p�҂b�c�A�ב��l�b�c
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Sel_SyukkaIrainusi(string[] sUser, string[] sData)
		{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//			logFileOpen(sUser);
			logWriter(sUser, INF, "�˗���g�p���`�F�b�N�J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[2];
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//			// ����`�F�b�N
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
					+   "FROM \"�r�s�O�P�o�׃W���[�i��\" \n"
					+ " WHERE ����b�c           = '" + sData[0] +"' \n"
					+ "   AND ����b�c           = '" + sData[1] +"' \n"
					+ "   AND �ב��l�b�c         = '" + sData[2] +"'"
					+ "   AND �폜�e�f           = '0' \n";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

				reader.Read();
				if (reader.GetDecimal(0) == 0)
				{
					sRet[0] = "����I��";
					sRet[1] = "0";
				}
				else
				{
					sRet[0] = "�o�׃f�[�^�����݂��邽�ߍ폜�ł��܂���";
					sRet[1] = reader.GetDecimal(0).ToString().Trim();
				}
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END

				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				conn2 = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
			}
			
			return sRet;
		}
// ADD 2005.11.07 ���s�j�ɉ� �o�׃W���[�i���`�F�b�N�ǉ� END

// ADD 2006.08.10 ���s�j�R�{ ���p�҃R�[�h�擾�ǉ� START

		/*********************************************************************
		 * �˗���f�[�^�X�V
		 * �����F����b�c�A����b�c�A�ב��l�b�c...
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Upd_irainusi1(string[] sUser, string[] sData)
		{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//			logFileOpen(sUser);
			logWriter(sUser, INF, "�˗���X�V�J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[5];
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//// ADD 2005.05.23 ���s�j�����J ����`�F�b�N�ǉ� START
//			// ����`�F�b�N
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 ���s�j�����J ����`�F�b�N�ǉ� END

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				string cmdQuery 
					= "UPDATE �r�l�O�P�ב��l \n"
					+    "SET �J�i����           = '" + sData[1]  +"', \n"
					+        "�d�b�ԍ��P         = '" + sData[2]  +"', \n"
					+        "�d�b�ԍ��Q         = '" + sData[3]  +"', \n"
					+        "�d�b�ԍ��R         = '" + sData[4]  +"', \n"
					+        "�X�֔ԍ�           = '" + sData[5]  + sData[6] + "', \n"
					+        "�Z���P             = '" + sData[7]  +"', \n"
					+        "�Z���Q             = '" + sData[8]  +"', \n"
					+        "���O�P             = '" + sData[9]  +"', \n"
					+        "���O�Q             = '" + sData[10] +"', \n"
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
					;
				string s�d�ʓ��͐��� = (sData.Length > 23) ? sData[23] : "0";
				if(s�d�ʓ��͐��� == "1"){
					cmdQuery = cmdQuery
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
// MOD 2005.05.17 ���s�j�����J �ː����� START
					+        "�ː�               =  " + sData[11] +", \n"
// MOD 2005.05.17 ���s�j�����J �ː����� END
					+        "�d��               =  " + sData[12] +", \n"
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
					;
				}
				cmdQuery = cmdQuery
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
					+        "\"���[���A�h���X\" = '" + sData[13] +"', \n"
					+        "���Ӑ�b�c         = '" + sData[19] +"', \n"
					+        "���Ӑ敔�ۂb�c     = '" + sData[20] +"', \n"
					+        "�폜�e�f           = '0', \n"
					+        "�X�V�o�f           = '" + sData[14] +"', \n"
					+        "�X�V��             = '" + sData[15] +"', \n"
					+        "�X�V����           =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
					+ " WHERE ����b�c           = '" + sData[16] +"' \n"
					+ "   AND ����b�c           = '" + sData[17] +"' \n"
					+ "   AND �ב��l�b�c         = '" + sData[0] +"' \n"
					+ "   AND �X�V����           =  " + sData[18] +"";

				int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);

				if(iUpdRow == 0)
					sRet[0] = "�f�[�^�ҏW���ɑ��̒[�����X�V����Ă��܂��B\r\n�ēx�A�ŐV�f�[�^���Ăяo���čX�V���Ă��������B";
				else
				{
					sRet[0] = "����I��";
					if(sData[22] == "1")
					{
						bool riyou = Upd_riyou(sUser, conn2, sData[16], sData[15], "            ", sData[14]);
						if(riyou == false)
							sRet[0] = "�f�t�H���g�ɐݒ�ł��܂���ł���";
					}
					else
					{
						if(sData[21] == "1")
						{
							bool riyou = Upd_riyou(sUser, conn2, sData[16], sData[15], sData[0], sData[14]);
							if(riyou == false)
								sRet[0] = "�f�t�H���g�ɐݒ�ł��܂���ł���";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				conn2 = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
			}
			
			return sRet;
		}


		/*********************************************************************
		 * ���p�҃R�[�h�擾
		 * �����F����b�c�A���p�҂b�c
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Get_riyo(string[] sUser, string sKcode, string sRcode)
		{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//			logFileOpen(sUser);
			logWriter(sUser, INF, "���p�҃R�[�h�擾�J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[18];

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//			// ����`�F�b�N
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
					= "SELECT �ב��l�b�c \n"
					+    "FROM �b�l�O�S���p�� \n"
					+  "WHERE ����b�c       = '" + sKcode + "'  \n"
					+    "AND ���p�҂b�c     = '" + sRcode + "'  ";

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

				bool bRead = reader.Read();
				if(bRead == true)
				{
					sRet[0]  = reader.GetString(0).Trim();
				}
				else
				{
					sRet[0] = "�Y���f�[�^����";
				}
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END

				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				conn2 = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
			}
			
			return sRet;
		}
// ADD 2006.08.10 ���s�j�R�{ ���p�҃R�[�h�擾�ǉ� END
// MOD 2010.09.08 ���s�j���� �b�r�u�o�͋@�\�̒ǉ� START
		/*********************************************************************
		 * �b�r�u�o��
		 * �����F����b�c�A���p�҂b�c
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Get_csvwrite(string[] sUser, string sKCode, string sBCode)
		{
			logWriter(sUser, INF, "�b�r�u�o�͊J�n");
			string[] sKey = new string[]{sKCode, sBCode};
			return Get_csvwrite2(sUser, sKey);
		}
		/*********************************************************************
		 * �b�r�u�o�͂Q
		 * �����F����b�c�A���p�҂b�c
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Get_csvwrite2(string[] sUser, string[] sKey)
		{
			string s����b�c     = (sKey.Length >  0) ? sKey[ 0] : "";
			string s����b�c     = (sKey.Length >  1) ? sKey[ 1] : "";
			string s�˗���J�i   = (sKey.Length >  2) ? sKey[ 2] : "";
			string s�˗���R�[�h = (sKey.Length >  3) ? sKey[ 3] : "";
			string s�˗��喼�O   = (sKey.Length >  4) ? sKey[ 4] : "";
			string s������b�c   = (sKey.Length >  5) ? sKey[ 5] : "";
			string s���ۂb�c     = (sKey.Length >  6) ? sKey[ 6] : "";
			string s�K�w���X�g�P = (sKey.Length >  7) ? sKey[ 7] : "2";
			string s�\�[�g�����P = (sKey.Length >  8) ? sKey[ 8] : "0";
			string s�K�w���X�g�Q = (sKey.Length >  9) ? sKey[ 9] : "0";
			string s�\�[�g�����Q = (sKey.Length > 10) ? sKey[10] : "0";

			int i�K�w���X�g�P = 2;
			int i�\�[�g�����P = 0;
			int i�K�w���X�g�Q = 0;
			int i�\�[�g�����Q = 0;
			try{
				i�K�w���X�g�P = int.Parse(s�K�w���X�g�P);
				i�\�[�g�����P = int.Parse(s�\�[�g�����P);
				i�K�w���X�g�Q = int.Parse(s�K�w���X�g�Q);
				i�\�[�g�����Q = int.Parse(s�\�[�g�����Q);
			}catch(Exception){
				;
			}

			if(sKey.Length >  2){
				logWriter(sUser, INF, "�b�r�u�o�͂Q�J�n");
			}
			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();

			string[] sRet = new string[1];
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			StringBuilder sbQuery = new StringBuilder(1024);
			try
			{
				sbQuery.Append("SELECT SM01.�ב��l�b�c \n");
				sbQuery.Append(      ",SM01.�d�b�ԍ��P \n");
				sbQuery.Append(      ",SM01.�d�b�ԍ��Q \n");
				sbQuery.Append(      ",SM01.�d�b�ԍ��R \n");
				sbQuery.Append(      ",SM01.�Z���P \n");
				sbQuery.Append(      ",SM01.�Z���Q \n");
				sbQuery.Append(      ",SM01.�Z���R \n");
				sbQuery.Append(      ",SM01.���O�P \n");
				sbQuery.Append(      ",SM01.���O�Q \n");
				sbQuery.Append(      ",SM01.���O�R \n");
				sbQuery.Append(      ",SM01.�X�֔ԍ� \n");
				sbQuery.Append(      ",SM01.�J�i���� \n");
				sbQuery.Append(      ",SM01.�ː� \n");
				sbQuery.Append(      ",SM01.�d�� \n");
				sbQuery.Append(      ",SM01.\"���[���A�h���X\" \n");
				sbQuery.Append(      ",SM01.���Ӑ�b�c \n");
				sbQuery.Append(      ",SM01.���Ӑ敔�ۂb�c \n");
				sbQuery.Append(      ",NVL(SM04.���Ӑ敔�ۖ�,' ') \n");
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
				sbQuery.Append(      ",NVL(CM01.�ۗ�����e�f,'0') \n");
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
				sbQuery.Append( "FROM �r�l�O�P�ב��l SM01 \n");
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
				sbQuery.Append(" LEFT JOIN �b�l�O�P��� CM01 \n");
				sbQuery.Append(" ON SM01.����b�c = CM01.����b�c \n");
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
				sbQuery.Append(" LEFT JOIN \"�b�l�O�Q����\" CM02 \n");
				sbQuery.Append(" ON SM01.����b�c = CM02.����b�c ");
				sbQuery.Append("AND SM01.����b�c = CM02.����b�c ");
				sbQuery.Append("AND CM02.�폜�e�f = '0' \n");
				sbQuery.Append(" LEFT JOIN \"�r�l�O�S������\" SM04 \n");
				sbQuery.Append(" ON CM02.�X�֔ԍ� = SM04.�X�֔ԍ� ");
				sbQuery.Append("AND SM01.���Ӑ�b�c = SM04.���Ӑ�b�c ");
				sbQuery.Append("AND SM01.���Ӑ敔�ۂb�c = SM04.���Ӑ敔�ۂb�c ");
// MOD 2011.03.09 ���s�j���� ������}�X�^�̎�L�[��[����b�c]��ǉ� START
				sbQuery.Append("AND SM01.����b�c = SM04.����b�c ");
// MOD 2011.03.09 ���s�j���� ������}�X�^�̎�L�[��[����b�c]��ǉ� END
				sbQuery.Append("AND SM04.�폜�e�f = '0' \n");
				sbQuery.Append("WHERE SM01.����b�c = '" + s����b�c + "' \n");
				sbQuery.Append(  "AND SM01.����b�c = '" + s����b�c + "' \n");

				if(s�˗���J�i.Length > 0){
					sbQuery.Append(" AND SM01.�J�i���� LIKE '"+ s�˗���J�i + "%' \n");
				}
				if(s�˗���R�[�h.Length > 0){
					sbQuery.Append(" AND SM01.�ב��l�b�c LIKE '"+ s�˗���R�[�h + "%' \n");
				}
				if(s�˗��喼�O.Length > 0){
					sbQuery.Append(" AND SM01.���O�P LIKE '%"+ s�˗��喼�O + "%' \n");
				}
				if(s������b�c.Length > 0){
					sbQuery.Append(" AND SM01.���Ӑ�b�c = '"+ s������b�c + "' \n");
					if(s���ۂb�c.Length > 0){
						sbQuery.Append(" AND SM01.���Ӑ敔�ۂb�c = '"+ s���ۂb�c + "' \n");
					}else{
						sbQuery.Append(" AND SM01.���Ӑ敔�ۂb�c = ' ' \n");
					}
				}
				sbQuery.Append(  "AND SM01.�폜�e�f = '0' \n");
				sbQuery.Append(  "ORDER BY \n");

				switch(i�K�w���X�g�P){
				case 1:
					sbQuery.Append(" SM01.�J�i���� ");
					if(i�\�[�g�����P == 1) sbQuery.Append(" DESC ");
					break;
				case 2:
					sbQuery.Append(" SM01.�ב��l�b�c");
					if(i�\�[�g�����P == 1) sbQuery.Append(" DESC ");
					break;
				case 3:
//					sbQuery.Append(" SM01.���Ӑ�b�c ");
//					if(i�\�[�g�����P == 1) sbQuery.Append(" DESC ");
//					sbQuery.Append(", SM01.���Ӑ敔�ۂb�c ");
					sbQuery.Append(" NVL(SM04.���Ӑ敔�ۖ�,SM01.���Ӑ�b�c || SM01.���Ӑ敔�ۂb�c) ");
					if(i�\�[�g�����P == 1) sbQuery.Append(" DESC ");
					break;
				case 4:
					sbQuery.Append(" SM01.���O�P ");
					if(i�\�[�g�����P == 1) sbQuery.Append(" DESC ");
//					sbQuery.Append(", SM01.���O�Q ");
//					if(i�\�[�g�����P == 1) sbQuery.Append(" DESC ");
					break;
				case 5:
					sbQuery.Append(" SM01.�d�b�ԍ��P ");
					if(i�\�[�g�����P == 1) sbQuery.Append(" DESC ");
					sbQuery.Append(", SM01.�d�b�ԍ��Q ");
					if(i�\�[�g�����P == 1) sbQuery.Append(" DESC ");
					sbQuery.Append(", SM01.�d�b�ԍ��R ");
					if(i�\�[�g�����P == 1) sbQuery.Append(" DESC ");
					break;
				case 6:
					sbQuery.Append(" SM01.�o�^���� ");
					if(i�\�[�g�����P == 1) sbQuery.Append(" DESC ");
					break;
				case 7:
					sbQuery.Append(" SM01.�X�V���� ");
					if(i�\�[�g�����P == 1) sbQuery.Append(" DESC ");
					break;
				}
				if(i�K�w���X�g�P != 0 && i�K�w���X�g�Q != 0){
					sbQuery.Append(",");
				}
				switch(i�K�w���X�g�Q){
				case 1:
					sbQuery.Append(" SM01.�J�i���� ");
					if(i�\�[�g�����Q == 1) sbQuery.Append(" DESC ");
					break;
				case 2:
					sbQuery.Append(" SM01.�ב��l�b�c");
					if(i�\�[�g�����Q == 1) sbQuery.Append(" DESC ");
					break;
				case 3:
//					sbQuery.Append(" SM01.���Ӑ�b�c ");
//					if(i�\�[�g�����Q == 1) sbQuery.Append(" DESC ");
//					sbQuery.Append(", SM01.���Ӑ敔�ۂb�c ");
					sbQuery.Append(" NVL(SM04.���Ӑ敔�ۖ�,SM01.���Ӑ�b�c || SM01.���Ӑ敔�ۂb�c) ");
					if(i�\�[�g�����Q == 1) sbQuery.Append(" DESC ");
					break;
				case 4:
					sbQuery.Append(" SM01.���O�P ");
					if(i�\�[�g�����Q == 1) sbQuery.Append(" DESC ");
//					sbQuery.Append(", SM01.���O�Q ");
//					if(i�\�[�g�����Q == 1) sbQuery.Append(" DESC ");
					break;
				case 5:
					sbQuery.Append(" SM01.�d�b�ԍ��P ");
					if(i�\�[�g�����Q == 1) sbQuery.Append(" DESC ");
					sbQuery.Append(", SM01.�d�b�ԍ��Q ");
					if(i�\�[�g�����Q == 1) sbQuery.Append(" DESC ");
					sbQuery.Append(", SM01.�d�b�ԍ��R ");
					if(i�\�[�g�����Q == 1) sbQuery.Append(" DESC ");
					break;
				case 6:
					sbQuery.Append(" SM01.�o�^���� ");
					if(i�\�[�g�����Q == 1) sbQuery.Append(" DESC ");
					break;
				case 7:
					sbQuery.Append(" SM01.�X�V���� ");
					if(i�\�[�g�����Q == 1) sbQuery.Append(" DESC ");
					break;
				}
				if(i�K�w���X�g�P == 0 && i�K�w���X�g�Q == 0){
					sbQuery.Append(" SM01.���O�P \n");
				}
				if(i�K�w���X�g�P != 2 && i�K�w���X�g�Q != 2){
					sbQuery.Append(", SM01.�ב��l�b�c \n");
				}
// MOD 2009.11.30 ���s�j���� ���������ɖ��O�A�������ǉ� END

				OracleDataReader reader;
				reader = CmdSelect(sUser, conn2, sbQuery);

				StringBuilder sbData;
				while (reader.Read()){
					sbData = new StringBuilder(1024);
					sbData.Append(sDbl + sSng + reader.GetString(0).TrimEnd() + sDbl);	// �ב��l�b�c
					sbData.Append(sKanma + sDbl + sSng);
					if(reader.GetString(1).TrimEnd().Length > 0){
						sbData.Append("(" + reader.GetString(1).TrimEnd() + ")");		// �d�b�ԍ��P
					}
					if(reader.GetString(2).TrimEnd().Length > 0){
						sbData.Append(reader.GetString(2).TrimEnd() + "-");				// �d�b�ԍ��Q
					}
					sbData.Append(reader.GetString(3).TrimEnd() + sDbl);				// �d�b�ԍ��R
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(4).TrimEnd() + sDbl);	// �Z���P
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(5).TrimEnd() + sDbl);	// �Z���Q
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(6).TrimEnd() + sDbl);	// �Z���R
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(7).TrimEnd() + sDbl);// ���O�P
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(8).TrimEnd() + sDbl);// ���O�Q
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(9).TrimEnd() + sDbl);// ���O�R
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(10).TrimEnd() + sDbl);// �X�֔ԍ�
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(11).TrimEnd() + sDbl);// �J�i����
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
					string s�d�ʓ��͐��� = reader.GetString(18).TrimEnd();
					if(s�d�ʓ��͐��� == "1"){
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
						sbData.Append(sKanma + sDbl + sSng + reader.GetDecimal(12).ToString().TrimEnd() + sDbl);// �ː�
						sbData.Append(sKanma + sDbl + sSng + reader.GetDecimal(13).ToString().TrimEnd() + sDbl);// �d��
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
					}else{
						sbData.Append(sKanma + sDbl + sSng + "0" + sDbl);// �ː�
						sbData.Append(sKanma + sDbl + sSng + "0" + sDbl);// �d��
					}
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(14).TrimEnd() + sDbl);// ���[���A�h���X
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(15).TrimEnd() + sDbl);// ���Ӑ�b�c
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(16).TrimEnd() + sDbl);// ���Ӑ敔�ۂb�c
					sList.Add(sbData);
				}
				disposeReader(reader);
				reader = null;

				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "�Y���f�[�^������܂���";
				else
				{
					sRet[0] = "����I��";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}
// MOD 2010.09.08 ���s�j���� �b�r�u�o�͋@�\�̒ǉ� END
// MOD 2010.09.08 ���s�j���� �b�r�u�捞�@�\�̒ǉ� START
		/*********************************************************************
		 * �A�b�v���[�h�f�[�^�ǉ��Q
		 * �����F����b�c�A����b�c�A�׎�l�b�c...
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		private static string INS_UPLOADDATA2_SELECT1
			= "SELECT 1 \n"
			+ " FROM �b�l�P�S�X�֔ԍ� \n"
			;

		private static string INS_UPLOADDATA2_SELECT2
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
//			= "SELECT �X�֔ԍ� \n"
//			+ " FROM �b�l�O�Q���� \n"
			= "SELECT CM02.�X�֔ԍ� \n"
			+ ", NVL(CM01.�ۗ�����e�f,'0') \n"
			+ " FROM �b�l�O�Q���� CM02 \n"
			+ " LEFT JOIN �b�l�O�P��� CM01 \n"
			+ " ON CM02.����b�c = CM01.����b�c \n"
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
			;

		private static string INS_UPLOADDATA2_SELECT3
			= "SELECT 1 \n"
			+ " FROM �r�l�O�S������ \n"
			;

		[WebMethod]
		public String[] Ins_uploadData2(string[] sUser, string[] sList)
		{
			logWriter(sUser, INF, "�A�b�v���[�h�f�[�^�ǉ��Q�J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[(sList.Length*2) + 1];

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null){
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();
			OracleDataReader reader;
			string cmdQuery = "";

			sRet[0] = "";
			try{
				string s���X�֔ԍ� = "";
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
				string s�d�ʓ��͐��� = "0";
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
				for (int iRow = 0; iRow < sList.Length; iRow++){
					sRet[iRow*2+1] = "";
					sRet[iRow*2+2] = "";

					string[] sData = sList[iRow].Split(',');
					if(sData.Length != 21){
						throw new Exception("�p�����[�^���G���[["+sData.Length+"]");
					}

					string s����b�c   = sData[0];
					string s����b�c   = sData[1];
					string s�ב��l�b�c = sData[2];
					string s�X�֔ԍ�   = sData[12];
					string s������b�c = sData[17];
					string s�����敔�� = sData[18];

					if(iRow == 0){
						//����}�X�^�̑��݃`�F�b�N
						cmdQuery = INS_UPLOADDATA2_SELECT2
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
//								+ "WHERE ����b�c = '" + s����b�c + "' \n"
//								+ "AND ����b�c = '" + s����b�c + "' \n"
//								+ "AND �폜�e�f = '0' \n"
								+ "WHERE CM02.����b�c = '" + s����b�c + "' \n"
								+ "AND CM02.����b�c = '" + s����b�c + "' \n"
								+ "AND CM02.�폜�e�f = '0' \n"
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
								;

						reader = CmdSelect(sUser, conn2, cmdQuery);
						if(!reader.Read()){
							reader.Close();
							disposeReader(reader);
							reader = null;
							throw new Exception("�Z�N�V���������݂��܂���");
						}
						s���X�֔ԍ� = reader.GetString(0).TrimEnd();
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
						s�d�ʓ��͐��� = reader.GetString(1).TrimEnd();
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
						reader.Close();
						disposeReader(reader);
						reader = null;
					}

					//�X�֔ԍ��}�X�^�̑��݃`�F�b�N
					cmdQuery = INS_UPLOADDATA2_SELECT1
// MOD 2010.09.29 ���s�j���� �X�֔ԍ�(__)�Ή��i�������o�O���������j START
//							+ "WHERE �X�֔ԍ� = '" + s�X�֔ԍ� + "' \n"
//							+ "AND �폜�e�f = '0' \n"
							;
							string s�X�֔ԍ��P = "";
							string s�X�֔ԍ��Q = "";
							if(s�X�֔ԍ�.Length > 3){
								s�X�֔ԍ��P = s�X�֔ԍ�.Substring(0,3).Trim();
								s�X�֔ԍ��Q = s�X�֔ԍ�.Substring(3).Trim();
								s�X�֔ԍ� = s�X�֔ԍ��P + s�X�֔ԍ��Q;
							}
							if(s�X�֔ԍ�.Length == 7){
								cmdQuery += " WHERE �X�֔ԍ� = '" + s�X�֔ԍ� + "' \n";
							}else{
								cmdQuery += " WHERE �X�֔ԍ� LIKE '" + s�X�֔ԍ� + "%' \n";
							}
							cmdQuery += "AND �폜�e�f = '0' \n"
// MOD 2010.09.29 ���s�j���� �X�֔ԍ�(__)�Ή��i�������o�O���������j END
							;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					if(!reader.Read()){
						sRet[iRow*2+1] = s�X�֔ԍ�.TrimEnd();//�Y���f�[�^����
//						reader.Close();
//						disposeReader(reader);
//						reader = null;
//						continue;
					}
					reader.Close();
					disposeReader(reader);
					reader = null;

					//������}�X�^�̑��݃`�F�b�N
					cmdQuery = INS_UPLOADDATA2_SELECT3
							+ "WHERE �X�֔ԍ� = '" + s���X�֔ԍ� + "' \n"
							+ "AND ���Ӑ�b�c = '" + s������b�c + "' \n"
							+ "AND ���Ӑ敔�ۂb�c = '" + s�����敔�� + "' \n"
// MOD 2011.03.09 ���s�j���� ������}�X�^�̎�L�[��[����b�c]��ǉ� START
							+ "AND ����b�c = '" + s����b�c + "' \n"
// MOD 2011.03.09 ���s�j���� ������}�X�^�̎�L�[��[����b�c]��ǉ� END
 							+ "AND �폜�e�f = '0' \n"
							;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					if(!reader.Read()){
						sRet[iRow*2+2] = s������b�c.TrimEnd(); //�Y���f�[�^����
						if(s�����敔��.TrimEnd().Length > 0){
							sRet[iRow*2+2] += "-" + s�����敔��.TrimEnd();
						}
//						reader.Close();
//						disposeReader(reader);
//						reader = null;
//						continue;
					}
					reader.Close();
					disposeReader(reader);
					reader = null;
					
					//�G���[������΁A���̍s
					if(sRet[iRow*2+1].Length != 0 || sRet[iRow*2+2].Length != 0){
						continue;
					}

					cmdQuery
						= "SELECT �폜�e�f \n"
						+   "FROM �r�l�O�P�ב��l \n"
						+  "WHERE ����b�c = '" + s����b�c + "' \n"
						+    "AND ����b�c = '" + s����b�c + "' \n"
						+    "AND �ב��l�b�c = '" + s�ב��l�b�c + "' "
						+    "FOR UPDATE "
						;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					int iCnt = 1;
					string s�폜�e�f = "";
					while (reader.Read()){
						s�폜�e�f = reader.GetString(0);
						iCnt++;
					}
					reader.Close();
					disposeReader(reader);
					reader = null;

					if(iCnt == 1){
						//�ǉ�
						cmdQuery 
							= "INSERT INTO �r�l�O�P�ב��l \n"
							+ "VALUES ( \n"
							+  "'" + sData[0] + "', "		//����b�c
							+  "'" + sData[1] + "', \n"		//����b�c
							+  "'" + sData[2] + "', \n"		//�ב��l�b�c

							+  "'" + sData[17] + "', "		//���Ӑ�b�c
							+  "'" + sData[18] + "', \n"	//���Ӑ敔�ۂb�c
							+  "'" + sData[3] + "', "		//�d�b�ԍ�
							+  "'" + sData[4] + "', "
							+  "'" + sData[5] + "', \n"
							+  "' ', "						//�e�`�w�ԍ�
							+  "' ', "
							+  "' ', \n"
							+  "'" + sData[6] + "', "		//�Z��
							+  "'" + sData[7] + "', "
							+  "'" + sData[8] + "', \n"
							+  "'" + sData[9] + "', "		//���O
							+  "'" + sData[10] + "', "
							+  "'" + sData[11] + "', \n"
							+  "'" + sData[12] + "', "		//�X�֔ԍ�
							+  "'" + sData[13] + "', \n"	//�J�i����
							+  " " + sData[14] + " , "		//�ː�
							+  " " + sData[15] + " , \n"	//�d��
							+  "' ', "						//�׎D�敪
							+  "'" + sData[16] + "', \n"	//���[���A�h���X
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
						//�㏑���X�V
						cmdQuery
							= "UPDATE �r�l�O�P�ב��l \n"
							+    "SET ���Ӑ�b�c = '" + sData[17] + "' \n"
							+       ",���Ӑ敔�ۂb�c = '" + sData[18] + "' \n"
							+       ",�d�b�ԍ��P = '" + sData[3] + "' \n"
							+       ",�d�b�ԍ��Q = '" + sData[4] + "' \n"
							+       ",�d�b�ԍ��R = '" + sData[5] + "' \n"
							+       ",�e�`�w�ԍ��P = ' ' \n"
							+       ",�e�`�w�ԍ��Q = ' ' \n"
							+       ",�e�`�w�ԍ��R = ' ' \n"
							+       ",�Z���P = '" + sData[6] + "' \n"
							+       ",�Z���Q = '" + sData[7] + "' \n"
							+       ",�Z���R = '" + sData[8] + "' \n"
							+       ",���O�P = '" + sData[9] + "' \n"
							+       ",���O�Q = '" + sData[10] + "' \n"
							+       ",���O�R = '" + sData[11] + "' \n"
							+       ",�X�֔ԍ� = '" + sData[12] + "' "
							+       ",�J�i���� = '" + sData[13] + "' "
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
							;
						if(s�d�ʓ��͐��� == "1"){
							cmdQuery = cmdQuery
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
							+       ",�ː� = "+ sData[14] +" "
							+       ",�d�� = "+ sData[15] +" "
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
							;
						}
						cmdQuery = cmdQuery
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
							+       ",�׎D�敪 = ' ' "
							+       ",\"���[���A�h���X\" = '"+ sData[16] +"' "
							+       ",�폜�e�f = '0' \n"
							;
						if(s�폜�e�f == "1"){
							cmdQuery
								+=  ",�o�^���� = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
								+   ",�o�^�o�f = '" + sData[19] + "' "
								+   ",�o�^�� = '" + sData[20] + "' \n"
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
								;
							if(s�d�ʓ��͐��� != "1"){
								cmdQuery = cmdQuery
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
								+   ",�ː� = "+ sData[14] +" "
								+   ",�d�� = "+ sData[15] +" \n"
								;
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� START
							}
// MOD 2011.05.06 ���s�j���� ���q�l���Ƃɏd�ʓ��͐��� END
						}
						cmdQuery
							+=      ",�X�V���� = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
							+       ",�X�V�o�f = '" + sData[19] + "' "
							+       ",�X�V�� = '" + sData[20] + "' \n"
							+ "WHERE ����b�c = '" + sData[0] + "' \n"
							+   "AND ����b�c = '" + sData[1] + "' \n"
							+   "AND �ב��l�b�c = '" + sData[2] + "' "
							;

							CmdUpdate(sUser, conn2, cmdQuery);
					}
					disposeReader(reader);
					reader = null;
				}
				logWriter(sUser, INF, "����I��");
				tran.Commit();
			}catch (OracleException ex){
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}catch (Exception ex){
				tran.Rollback();
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}finally{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}
// MOD 2010.09.08 ���s�j���� �b�r�u�捞�@�\�̒ǉ� END
// MOD 2010.09.17 ���s�j���� �������폜�@�\�̒ǉ� START
		/*********************************************************************
		 * �͂���f�[�^�폜
		 * �����F����b�c�A����b�c�A�׎�l�b�c�A�X�V�o�f�A�X�V��
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Del_irainusis(string[] sUser, string[] sData, string[] sList)
		{
			logWriter(sUser, INF, "�˗��啡�����폜�J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[sList.Length + 1];
			sRet[0] = "";
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null){
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try{
				string cmdQuery; 
// MOD 2011.01.18 ���s�j���� �������폜�@�\�̏�Q�Ή� START
				int  iCntUnDelete = 0;
// MOD 2011.01.18 ���s�j���� �������폜�@�\�̏�Q�Ή� END
				for(int iCnt = 0; iCnt < sList.Length; iCnt++){
					sRet[iCnt + 1] = "";
					if(sList[iCnt] == null) continue;
					if(sList[iCnt].Length == 0) continue;
// MOD 2011.01.18 ���s�j���� �������폜�@�\�̏�Q�Ή� START
					cmdQuery 
					= "SELECT COUNT(*) \n" 
					+   "FROM \"�r�s�O�P�o�׃W���[�i��\" \n"
					+ " WHERE ����b�c           = '" + sData[0] +"' \n"
					+ "   AND ����b�c           = '" + sData[1] +"' \n"
					+ "   AND �ב��l�b�c         = '" + sList[iCnt] +"'"
					+ "   AND �폜�e�f           = '0' \n";
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
// MOD 2011.01.18 ���s�j���� �������폜�@�\�̏�Q�Ή� END
					cmdQuery 
						= "UPDATE �r�l�O�P�ב��l \n"
						+    "SET �폜�e�f           = '1', \n"
						+        "�X�V�o�f           = '" + sData[3] +"', \n"
						+        "�X�V��             = '" + sData[4] +"', \n"
						+        "�X�V����           =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
						+ " WHERE ����b�c           = '" + sData[0] +"' \n"
						+ "   AND ����b�c           = '" + sData[1] +"' \n"
						+ "   AND �ב��l�b�c         = '" + sList[iCnt] +"'";
					int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
					sRet[iCnt + 1] = iUpdRow.ToString();
				}

				tran.Commit();				
				sRet[0] = "����I��";
// MOD 2011.01.18 ���s�j���� �������폜�@�\�̏�Q�Ή� START
				if(iCntUnDelete > 0){
					sRet[0] = "�o�׃f�[�^�����݂��邽�ߍ폜�ł��܂���[ "+iCntUnDelete+"��]";
				}
// MOD 2011.01.18 ���s�j���� �������폜�@�\�̏�Q�Ή� END
			}catch (OracleException ex){
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}catch (Exception ex){
				tran.Rollback();
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}finally{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			
			return sRet;
		}
// MOD 2010.09.17 ���s�j���� �������폜�@�\�̒ǉ� END
	}
}
