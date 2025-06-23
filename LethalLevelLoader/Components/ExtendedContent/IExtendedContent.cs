using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace LethalLevelLoader
{
    public interface IExtendedContent<E,C,M> where E : ExtendedContent<E,C,M>, IExtendedContent<E,C,M> where M : ExtendedContentManager, IExtendedManager<E,C,M>
    {
        public C Content { get; }

    }

    public interface IExtendedContent;
    public interface IExtendedContent<C> : IExtendedContent
    {
        public C GetContent();
    }
    public interface IManagedContent;
    public interface IContentManager;
    public interface IContentManager<E> : IContentManager where E : UnityEngine.Object, IManagedContent;
    public interface IManagedContent<M> : IManagedContent where M : UnityEngine.Object, IContentManager;

    public abstract class BaseBaseManager<E> : MonoBehaviour, IContentManager<E> where E : UnityEngine.Object, IManagedContent, IExtendedContent
    {
        private static List<E> ExtendedContents = new List<E>();
        public static List<E> GetExtendedContents() => new List<E>(ExtendedContents);

        public abstract bool TryRegisterContent(E extendedContent);

        protected virtual void RegisterContent(E extendedContent)
        {
            ExtendedContents.Add(extendedContent);
        }
    }

    public class BaseManager<E,C> : BaseBaseManager<E>, IContentManager<E> where E : UnityEngine.Object, IManagedContent, IExtendedContent<C>
    {
        private static Dictionary<C,E> ExtendedContentsDict = new Dictionary<C,E>();
        public static bool TryGetExtendedContent(C content, out E extendedContent)
        {
            extendedContent = null;
            if (content == null || !ExtendedContentsDict.TryGetValue(content, out E returnC))
                return (false);
            extendedContent = returnC;
            return (true);
        }

        public override bool TryRegisterContent(E extendedContent)
        {
            //STUFF

            RegisterContent(extendedContent);

            return (true);
        }

        protected sealed override void RegisterContent(E extendedContent)
        {
            ExtendedContentsDict.Add(extendedContent.GetContent(), extendedContent);
            base.RegisterContent(extendedContent);
        }
    }

    public abstract class BaseBaseContent<C, M> : ScriptableObject, IManagedContent<M>, IExtendedContent<C> where M : UnityEngine.Object, IContentManager
    {
        public virtual void OnDoStuff() { }
        [field: SerializeField] public C Content { get; set; }
        public C GetContent() => Content;
    }

    public abstract class BaseContent<E,C,M> : BaseBaseContent<C,M>, IManagedContent<M>, IExtendedContent<C> where M : UnityEngine.Object, IContentManager where E : UnityEngine.Object, IExtendedContent<C>, IManagedContent<M>
    {
        public static List<E> Contents => BaseBaseManager<E>.GetExtendedContents();
    }

    public class Content
    {

    }

    public class CoolContent : BaseContent<CoolContent, Content, CoolContentManager>
    {

        public override void OnDoStuff()
        {
            foreach (CoolContent content in Contents)
            {

            }
        }
    }

    public class CoolContentManager : BaseManager<CoolContent, Content>
    {

    }

    public static class Global
    {
        public static List<E> GetContentsOfType<E>() where E : UnityEngine.Object, IManagedContent, IExtendedContent
        {
            return BaseBaseManager<E>.GetExtendedContents();
        }

        public static E GetContent<C,E>(C content) where E : UnityEngine.Object, IManagedContent, IExtendedContent<C>
        {
            if (content == null || !BaseManager<E, C>.TryGetExtendedContent(content, out E returnC))
                return (null);
            return (returnC);
        }

        public static bool TryGetContent<C, E>(C content, out E returnContent) where E : UnityEngine.Object, IManagedContent, IExtendedContent<C>
        {
            returnContent = GetContent<C,E>(content);
            return (returnContent != null);
        }

        public static void Test()
        {
            Content example = null;

            if (TryGetContent(example, out CoolContent extendedExample))
            {

            }
        }
    }
}
