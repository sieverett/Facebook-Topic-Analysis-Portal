import React from 'react';
import { formatDifference, showDate } from '../../Common/Utilities';
import DataTable from '../Common/Data/DataTable';
import ErrorPanel from '../Common/ErrorPanel';
import LoadingIndicator from '../Common/LoadingIndicator';

export default function PageScrapeList(props) {
  const { scrapes, errorMessage, onRowSelected } = props;
  const mapping = [
      { name: 'Date',  key: scrape => showDate(scrape.importStart)                                     },
      { name: 'Pages', key: scrape => scrape.pages.length                                              },
      { name: 'Took',  key: scrape => formatDifference(scrape.importStart, scrape.importEnd) + ' mins' }
  ];

  if (errorMessage) {
      return <ErrorPanel message={errorMessage} />;
  } else if (!scrapes.data) {
      return <LoadingIndicator />
  }

  return (
    <DataTable minSize={12} startIndex={scrapes.startItemIndex + 1}
                mapping={mapping} data={scrapes.data} onRowSelected={onRowSelected} />
  );
}
