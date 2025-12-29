//using Microsoft.Extensions.DependencyInjection;
//using ZiggyCreatures.Caching.Fusion;


//// PSEUDOCODE / PLAN:
//// 1. Define a sealed wrapper class `OltFusionCacheBuilder` that holds an `IFusionCacheBuilder` instance.
//// 2. Expose the inner `IFusionCacheBuilder` through a property so callers can access raw builder if needed.
//// 3. Provide a static factory `From` (or `Wrap`) to create the wrapper from an existing `IFusionCacheBuilder`.
//// 4. Provide fluent wrapper methods that call existing extension methods on the inner builder:
////    - `WithRedisBackplane(...)` delegating to the extension defined in this project.
////    - `WithRedisDistributedCache(...)` delegating as well.
////    - These wrapper methods return `OltFusionCacheBuilder` to allow chaining custom methods.
//// 5. Provide `Build()` (or `ToBuilder()`) to get the underlying `IFusionCacheBuilder` so it can be passed where required.
//// 6. Add an implicit conversion to `IFusionCacheBuilder` for convenience.
//// 7. Keep the class minimal so additional custom fluent methods can be added later.


//namespace OLT.Core
//{
//    /// <summary>
//    /// A lightweight wrapper around <see cref="IFusionCacheBuilder"/> that exposes fluent methods
//    /// and a place to add project-specific cache builder extensions.
//    /// </summary>
//    internal sealed class OltFusionCacheBuilder
//    {
//        /// <summary>
//        /// The inner fusion cache builder instance.
//        /// </summary>
//        public IFusionCacheBuilder Inner { get; }

//        public OltFusionCacheBuilder(IServiceCollection services, IFusionCacheBuilder inner)
//        {
//            Inner = inner ?? throw new ArgumentNullException(nameof(inner));
//        }

//        /// <summary>
//        /// Wraps an existing <see cref="IFusionCacheBuilder"/>.
//        /// </summary>
//        public static OltFusionCacheBuilder From(IFusionCacheBuilder builder) => new OltFusionCacheBuilder(builder);

//        /// <summary>
//        /// Returns the underlying <see cref="IFusionCacheBuilder"/>.
//        /// </summary>
//        public IFusionCacheBuilder Build() => Inner;

//        ///// <summary>
//        ///// Fluent wrapper that delegates to the existing WithRedisBackplane extension.
//        ///// Returns the wrapper to allow chaining custom methods.
//        ///// </summary>
//        //public OltFusionCacheBuilder WithRedisBackplane(OltRedisCacheOptions options, Action<ConfigurationOptions>? config = null)
//        //{
//        //    // Delegate to the extension method defined for IFusionCacheBuilder
//        //    Inner.WithRedisBackplane(options, config);
//        //    return this;
//        //}

//        ///// <summary>
//        ///// Fluent wrapper that delegates to the existing WithRedisDistributedCache extension.
//        ///// Returns the wrapper to allow chaining custom methods.
//        ///// </summary>
//        //public OltFusionCacheBuilder WithRedisDistributedCache(OltRedisCacheOptions options)
//        //{
//        //    Inner.WithRedisDistributedCache(options);
//        //    return this;
//        //}

//        /// <summary>
//        /// Example for adding custom project-specific fluent methods.
//        /// Add your custom builder methods here and use Inner to delegate or configure.
//        /// </summary>
//        public OltFusionCacheBuilder WithCacheKeyPrefix(string prefix)
//        {
//            if (!string.IsNullOrEmpty(prefix))
//            {
//                Inner.WithCacheKeyPrefix(prefix);
//            }

//            return this;
//        }

//        ///// <summary>
//        ///// Implicit conversion back to IFusionCacheBuilder for compatibility.
//        ///// </summary>
//        //public static implicit operator IFusionCacheBuilder(OltFusionCacheBuilder wrapper) => wrapper.Inner;
//    }
//}