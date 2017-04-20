import React from 'react';
import { showDate } from '../../Common/Utilities';
import DataTable from '../Common/Data/DataTable';
import ErrorPanel from '../Common/ErrorPanel';
import LoadingIndicator from '../Common/LoadingIndicator';

export default function PageList(props) {
  const { pages, errorMessage, onRowSelected } = props;
  const mapping = [
    { name: 'Name',            key: page => page.name                                    },
    { name: 'ID',              key: page => page.facebookId                              },
    { name: 'Number Of Likes', key: page => (page.fanCount || 0) + ' Likes'              },
    { name: 'First Scrape',    key: page => showDate(page.firstScrape, 'Never Scraped')  },
    { name: 'Latest Scrape',   key: page => showDate(page.latestScrape, 'Never Scraped') },
    { name: 'Added',           key: page => showDate(page.created)                       }
  ];

  if (errorMessage) {
    return <ErrorPanel message={errorMessage} />
  } else if (!pages.data) {
    return <LoadingIndicator />
  }

  return <DataTable minSize={12} startIndex={pages.startItemIndex + 1}
                    mapping={mapping} data={pages.data} onRowSelected={onRowSelected} />;
}
