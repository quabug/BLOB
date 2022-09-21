using System.Linq;
using AnyProcessor.CodeGen;
using AnySerialize;
using AnySerialize.CodeGen;
using Mono.Cecil;
using NUnit.Framework;
using OneShot;

namespace Blob.Tests
{
    public class TestBlobEditor : CecilTestBase
    {
        private TypeTree _typeTree;
        private Container _container;
        
        protected override void OnSetUp()
        {
            var serializerTypes = typeof(IAny).Assembly.GetTypes()
                .Concat(typeof(AnyManagedBlobReference<,>).Assembly.GetTypes())
                .Where(type => typeof(IAny).IsAssignableFrom(type))
            ;
            _typeTree = new TypeTree(serializerTypes.Select(type => ImportReference(type).Resolve()));
            
            _container = new Container();
            _container.RegisterInstance(_assemblyDefinition).AsSelf();
            _container.RegisterInstance(new ILPostProcessorLogger()).AsSelf();
            _container.RegisterInstance(_module).AsSelf();
            _container.RegisterInstance(_typeTree).AsSelf();
        }

        protected override void OnTearDown()
        {
            _container.Dispose();
        }

        private TypeReference SearchReadOnly<T>()
        {
            var target = ImportReference(typeof(IReadOnlyAny<T>));
            Assert.IsTrue(target is GenericInstanceType genericTarget && genericTarget.GenericArguments.Count == 1);
            var container = _container.CreateChildContainer();
            container.RegisterInstance(container).AsSelf();
            container.RegisterInstance(_module).AsSelf();
            return container.FindClosestType(target);
        }
        
        struct BlobData {}
        
        [Test]
        public void should_find_type_of_managed_blob_asset()
        {
            var value = SearchReadOnly<ManagedBlobAssetReference<BlobData>>();
        }
    }
}