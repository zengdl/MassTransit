// Copyright 2007-2015 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Pipeline
{
    using System;
    using System.Threading.Tasks;
    using GreenPipes.Util;
    using Util;


    public class SendMessageObservable<TMessage> :
        Connectable<ISendMessageObserver<TMessage>>,
        ISendMessageObserver<TMessage>
        where TMessage : class
    {
        public Task PreSend(SendContext<TMessage> context)
        {
            return ForEachAsync(x => x.PreSend(context));
        }

        public Task PostSend(SendContext<TMessage> context)
        {
            return ForEachAsync(x => x.PostSend(context));
        }

        public Task SendFault(SendContext<TMessage> context, Exception exception)
        {
            return ForEachAsync(x => x.SendFault(context, exception));
        }
    }
}