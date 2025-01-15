using System.Collections;
using System.Collections.Generic;
using Core.Event;
using QFramework;
using UnityEngine;

namespace Game
{
    public class MultilingualManager : AbstractSystem
    {
        private MultilingualData_Model m_MultilingualData_Model;
        
        private Multilingual_Utility m_Multilingual_Utility;
        
        protected override void OnInit()
        {
            m_MultilingualData_Model = this.GetModel<MultilingualData_Model>();
            m_Multilingual_Utility = this.GetUtility<Multilingual_Utility>();

            this.RegisterEvent<SOnChangeLanguageEvent>((data) =>
            {
                ChangeLanguage(data.willChangeLanguage);
            });
        }
        
        /// <summary>
        /// 切换语言
        /// </summary>
        /// <param name="newLanguageType"></param>
        private void ChangeLanguage(ELanguageType newLanguageType)
        {
            m_MultilingualData_Model.willChangeLanguageType = newLanguageType;
        }
    }
}

