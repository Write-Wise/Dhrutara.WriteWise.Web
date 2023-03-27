import useSWR from 'swr'
import { useRouter } from 'next/router';
 
function UserDeletionStatus() {
  
  const fetcher = (...args) => fetch(...args).then(res => res.text());

  const router = useRouter();
  const userid  = router.query['userid'] || '';
  const provider  = router.query['provider'] || '';

  if(IsNullOrWhitespace(userid) || IsNullOrWhitespace(provider)){
    return <div className='home'><h1>Invalid request!</h1></div>
  }
  
  const { data, error, isLoading } = useSWR(`/api/GetDeletedUserStatus?userId=${userid}&provider=${provider}`, fetcher)
 
  if (error) return <div className='home'><h1>Sorry, failed to fetch status. Please try again later. We have been informed about this issue.</h1></div>
  if (isLoading) return <div>Checking status...</div>
  return <div className='home'><h1>{data}</h1></div>
}

function IsNullOrWhitespace(text){
  if(!text){
    return true;
  }

  let result = text.replace(/^\s+|\s+$/gm,'');
  return result === "";
}

export default UserDeletionStatus