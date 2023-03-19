import useSWR from 'swr'
import { useRouter } from 'next/router';
 
function Profile() {
  
  const fetcher = (...args) => fetch(...args).then(res => res.text())
  const router = useRouter();
  let userid  = router.query['userid'] || '';
  let provider  = router.query['provider'] || '';
  
  const { data, error, isLoading } = useSWR(`/api/GetDeletedUserStatus?userId=${userid}&provider=${provider}`, fetcher)
 
  if (error) return <div>{error}</div>
  if (isLoading) return <div>loading...</div>
  return <div>{data}</div>
}

export default Profile