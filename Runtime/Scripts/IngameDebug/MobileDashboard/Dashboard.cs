using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ANU.IngameDebug.Console.Commands.Implementations;
using ANU.IngameDebug.Utils;
using UnityEngine;
using UnityEngine.Pool;

namespace ANU.IngameDebug.Console.Dashboard
{
    public class Dashboard : MonoBehaviour
    {
        [SerializeField] private Transform _content;
        [SerializeField] private GenericCommandPresenter _genericPresenterPrefab;

        private ObjectPool<GenericCommandPresenter> _genericPresentersPool;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(1f);

            _genericPresentersPool = new ObjectPool<GenericCommandPresenter>(
                () => Instantiate(_genericPresenterPrefab)
            );

            _content.DeleteAllChild();

            //TODO: group commands by Prefix

            foreach (var item in DebugConsole.Commands.Commands.Values.OfType<MemberCommand>())
            {
                // ignore reserved parameters

                // if we have 0 arguments - just show button
                // if we have 1 argument
                //      bool        - switch
                //      less than 4 vals - switch
                //      more than 4 vals - dropdown 
                //      generic val - input field
                // if we have more than 1 argument - show 2 buttons - with command name, and "..." for opening window to enter arguments. entered arguments should remember last input

                var presenter = _genericPresentersPool.Get();
                presenter.transform.SetParent(_content, false);
                presenter.Present(item);
            }
        }
    }
}